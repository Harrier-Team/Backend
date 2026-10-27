using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.Entities;
using Hackatohm.Api.Hubs;
using Hackatohm.Domain.QualityGates;
using Microsoft.AspNetCore.SignalR;
using Hackatohm.Api.Queues;
using Hackatohm.Application.MotorStatus;
using Microsoft.Extensions.Options;
using Hackatohm.Api.UseCases.Motores;

namespace Hackatohm.Api.UseCases.Leituras;

public enum StatusProcessamentoLote
{
    Sucesso,
    SensoresNaoCadastrados,
    LoteInvalido
}

public record ProcessarLeiturasLoteResult(
    StatusProcessamentoLote Status,
    string? MensagemErro = null,
    IReadOnlyList<string>? SensoresAusentes = null,
    int SensoresProcessados = 0,
    bool AlertaDetectado = false
)
{
    public static ProcessarLeiturasLoteResult CriarSucesso(int sensoresProcessados, bool alertaDetectado) =>
        new(StatusProcessamentoLote.Sucesso, null, null, sensoresProcessados, alertaDetectado);

    public static ProcessarLeiturasLoteResult CriarSensoresNaoCadastrados(IReadOnlyList<string> sensoresAusentes) =>
        new(StatusProcessamentoLote.SensoresNaoCadastrados,
            "Sensores não cadastrados.",
            sensoresAusentes);
}

public class ProcessarLeiturasLoteUseCase(
    ISensorRepository sensorRepository,
    ILeituraTemperaturaRepository leituraTemperaturaRepository,
    IUnitOfWork unitOfWork,
    IHubContext<TelemetriaHub, ITelemetriaClient> hubContext,
    IQualityGateEvaluator qualityGateEvaluator,
    IMotorStatusService motorStatusService,
    IOptions<TelemetriaOptions> telemetriaOptions,
    ILogger<ProcessarLeiturasLoteUseCase> logger)
{
    public async Task<ProcessarLeiturasLoteResult> ProcessarAsync(
        TelemetriaRecebida telemetria,
        CancellationToken cancellationToken = default)
    {
        var payload = telemetria.Payload;
        var ipAtual = telemetria.IpAtual;
        var dataRecebimento = telemetria.DataRecebimento;

        var idsSensores = payload.Sensores
            .Select(leitura => Sensor.CriarId(payload.EspMacAddress, leitura.NumeroEscova))
            .ToArray();

        var sensoresPorId = await sensorRepository.ObterPorIdsComoDicionarioAsync(
            idsSensores,
            cancellationToken);

        var sensoresAusentes = idsSensores
            .Where(id => !sensoresPorId.ContainsKey(id))
            .ToArray();

        if (sensoresAusentes.Length > 0)
        {
            logger.LogWarning(
                "Telemetria {RequestId} rejeitada para ESP {MacAddress}: sensores não cadastrados: {SensorIds}.",
                telemetria.RequestId,
                payload.EspMacAddress,
                string.Join(", ", sensoresAusentes));
            return ProcessarLeiturasLoteResult.CriarSensoresNaoCadastrados(sensoresAusentes);
        }

        var motores = sensoresPorId.Values.Select(sensor => sensor.MotorId).Distinct().ToArray();
        if (motores.Length != 1)
            return RejeitarLote("O lote deve conter sensores de um único motor.");

        var sensoresDoMotor = await sensorRepository.ObterPorMotorAsync(motores[0], cancellationToken);
        var idsEsperados = sensoresDoMotor.Select(sensor => sensor.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!idsEsperados.SetEquals(idsSensores))
            return RejeitarLote("O ESP deve enviar todos os sensores cadastrados do motor no mesmo lote.");

        var ultimasLeituras = await leituraTemperaturaRepository.ObterMaisRecentesPorSensoresAsync(
            idsSensores,
            cancellationToken);

        var leiturasParaSalvar = new List<LeituraTemperatura>(payload.Sensores.Count);
        var contextosAvaliacao = new List<ContextoAvaliacao>(payload.Sensores.Count);
        foreach (var leitura in payload.Sensores)
        {
            var sensor = sensoresPorId[Sensor.CriarId(payload.EspMacAddress, leitura.NumeroEscova)];
            sensor.UltimaConexao = dataRecebimento;
            if (ipAtual is not null)
                sensor.IpAtual = ipAtual;

            ultimasLeituras.TryGetValue(sensor.Id, out var leituraAnterior);

            contextosAvaliacao.Add(new ContextoAvaliacao(
                sensor,
                leitura,
                dataRecebimento,
                payload.Sensores,
                leituraAnterior?.ValorCelsius,
                leituraAnterior is null
                    ? null
                    : dataRecebimento - leituraAnterior.DataHora));
        }

        var resultadosPorSensor = await qualityGateEvaluator.AvaliarLoteAsync(
            contextosAvaliacao,
            cancellationToken);

        foreach (var contextoAvaliacao in contextosAvaliacao)
        {
            var sensor = contextoAvaliacao.Sensor;
            var leitura = contextoAvaliacao.LeituraAtual;
            var resultadosAvaliacao = resultadosPorSensor[sensor.Id];
            leiturasParaSalvar.Add(new LeituraTemperatura
            {
                SensorId = sensor.Id,
                ValorCelsius = leitura.Temperatura,
                DataHora = dataRecebimento,
                AlertaDesgaste = resultadosAvaliacao.Any(resultado => resultado.DisparouAlerta)
            });
        }

        await leituraTemperaturaRepository.AdicionarRangeAsync(
            leiturasParaSalvar,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Telemetria {RequestId} processada e salva para ESP {MacAddress}: {SensorCount} leituras de {ClientIp}.",
            telemetria.RequestId,
            payload.EspMacAddress,
            leiturasParaSalvar.Count,
            ipAtual ?? "IP indisponível");

        var dtosRealtime = leiturasParaSalvar
            .Select(leitura => TelemetriaRealtimeMapper.Mapear(
                leitura,
                payload.EspMacAddress,
                sensoresPorId[leitura.SensorId],
                resultadosPorSensor[leitura.SensorId]))
            .ToList();

        try
        {
            await hubContext.Clients.All.ReceberLoteTelemetria(dtosRealtime);

            foreach (var motorId in sensoresPorId.Values
                         .Select(sensor => sensor.MotorId)
                         .Distinct())
            {
                var leiturasDoMotor = leiturasParaSalvar
                    .Where(leitura => sensoresPorId[leitura.SensorId].MotorId == motorId)
                    .Select(leitura => new MotorStatusReading(
                        leitura.SensorId,
                        sensoresPorId[leitura.SensorId].NumeroEscova,
                        leitura.ValorCelsius,
                        leitura.DataHora,
                        leitura.AlertaDesgaste,
                        resultadosPorSensor[leitura.SensorId]))
                    .ToArray();
                var status = motorStatusService.Agregar(
                    motorId,
                    leiturasDoMotor,
                    TimeSpan.FromSeconds(telemetriaOptions.Value.OfflineAfterSeconds),
                    dataRecebimento);
                await hubContext.Clients.All
                    .ReceberStatusMotor(MotorStatusResponseMapper.Mapear(status));
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Telemetria persistida, mas não foi possível notificar os clientes SignalR. " +
                "ESP {MacAddress}, quantidade de leituras {SensorCount}.",
                payload.EspMacAddress,
                leiturasParaSalvar.Count);
        }

        return ProcessarLeiturasLoteResult.CriarSucesso(
            sensoresProcessados: leiturasParaSalvar.Count,
            alertaDetectado: leiturasParaSalvar.Any(leitura => leitura.AlertaDesgaste)
        );

        ProcessarLeiturasLoteResult RejeitarLote(string motivo)
        {
            logger.LogWarning("Telemetria {RequestId} descartada: {Motivo}", telemetria.RequestId, motivo);
            return new(StatusProcessamentoLote.LoteInvalido, motivo);
        }
    }

}
