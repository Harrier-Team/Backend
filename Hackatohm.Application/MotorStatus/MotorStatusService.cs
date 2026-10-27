using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.MotorStatus;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Application.MotorStatus;

public sealed class MotorStatusService(
    IMotorRepository motorRepository,
    ILeituraTemperaturaRepository leituraRepository,
    IQualityGateEvaluator qualityGateEvaluator) : IMotorStatusService
{
    public async Task<MotorStatusResult?> ObterAsync(
        int motorId,
        TimeSpan offlineAfter,
        DateTimeOffset agora,
        CancellationToken cancellationToken = default)
    {
        var motor = await motorRepository.ObterComSensoresAsync(motorId, cancellationToken);
        if (motor is null)
            return null;

        var sensores = motor.Sensores.OrderBy(sensor => sensor.NumeroEscova).ToArray();
        var leituras = await leituraRepository.ObterMaisRecentesPorSensoresAsync(
            sensores.Select(sensor => sensor.Id).ToArray(),
            cancellationToken);

        var contextos = new List<ContextoAvaliacao>();
        foreach (var sensor in sensores.Where(sensor => leituras.ContainsKey(sensor.Id)))
        {
            var leitura = leituras[sensor.Id];
            var anterior = await leituraRepository.ObterAnteriorPorSensorAsync(
                sensor.Id, leitura.DataHora, cancellationToken);
            var lote = await leituraRepository.ObterPorMotorNoInstanteAsync(
                motorId, leitura.DataHora, cancellationToken);

            contextos.Add(new ContextoAvaliacao(
                sensor,
                new LeituraTelemetriaDto
                {
                    NumeroEscova = sensor.NumeroEscova,
                    Temperatura = leitura.ValorCelsius
                },
                leitura.DataHora,
                lote.Select(item => new LeituraTelemetriaDto
                {
                    NumeroEscova = item.Sensor.NumeroEscova,
                    Temperatura = item.ValorCelsius
                }).ToArray(),
                anterior?.ValorCelsius,
                anterior is null ? null : leitura.DataHora - anterior.DataHora));
        }

        var gates = await qualityGateEvaluator.AvaliarLoteAsync(contextos, cancellationToken);
        var leiturasStatus = contextos
            .Select(contexto => new MotorStatusReading(
                contexto.Sensor.Id,
                contexto.Sensor.NumeroEscova,
                leituras[contexto.Sensor.Id].ValorCelsius,
                leituras[contexto.Sensor.Id].DataHora,
                gates[contexto.Sensor.Id].Any(gate => gate.DisparouAlerta),
                gates[contexto.Sensor.Id]))
            .ToArray();

        return Agregar(motorId, leiturasStatus, offlineAfter, agora, sensores.Length);
    }

    public MotorStatusResult Agregar(
        int motorId,
        IReadOnlyList<MotorStatusReading> leituras,
        TimeSpan offlineAfter,
        DateTimeOffset agora,
        int? totalSensores = null)
    {
        ArgumentNullException.ThrowIfNull(leituras);
        if (offlineAfter <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(offlineAfter));

        var escovas = leituras
            .Select(CriarEscovaResult)
            .ToArray();

        var severidadeGeral = escovas.Length == 0
            ? NivelSeveridade.Normal
            : escovas.Max(escova => escova.Severidade);
        var leiturasRecentes = escovas.Count(escova =>
            agora - escova.DataHora <= offlineAfter);
        var esperadas = Math.Max(totalSensores ?? escovas.Length, escovas.Length);
        var situacao = escovas.Length == 0 || leiturasRecentes == 0
            ? SituacaoMotor.Offline
            : leiturasRecentes == esperadas
                ? SituacaoMotor.Online
                : SituacaoMotor.Degradado;

        return new MotorStatusResult(
            motorId,
            severidadeGeral,
            situacao,
            CriarContagem(escovas),
            CriarMotivo(leituras),
            escovas);
    }

    private static EscovaStatusResult CriarEscovaResult(
        MotorStatusReading leitura)
    {
        var severidade = leitura.ResultadosQualityGates.Count == 0
            ? NivelSeveridade.Normal
            : leitura.ResultadosQualityGates.Max(gate => gate.Severidade);

        return new(
            leitura.SensorId,
            leitura.NumeroEscova,
            leitura.ValorCelsius,
            severidade,
            leitura.DataHora,
            leitura.AlertaDesgaste,
            leitura.ResultadosQualityGates);
    }

    private static SeveridadeContagemResult CriarContagem(
        IReadOnlyList<EscovaStatusResult> escovas) =>
        new(
            escovas.Count(escova => escova.Severidade == NivelSeveridade.Normal),
            escovas.Count(escova => escova.Severidade == NivelSeveridade.Atencao),
            escovas.Count(escova => escova.Severidade == NivelSeveridade.AlertaPreditivo),
            escovas.Count(escova => escova.Severidade == NivelSeveridade.Critico));

    private static MotivoStatusResult? CriarMotivo(
        IReadOnlyList<MotorStatusReading> leituras)
    {
        var candidatos = leituras
            .SelectMany(leitura => leitura.ResultadosQualityGates
                .Where(gate => gate.DisparouAlerta)
                .Select(gate => (leitura.NumeroEscova, Gate: gate)))
            .OrderByDescending(item => item.Gate.Severidade)
            .ThenBy(item => item.NumeroEscova)
            .ToArray();

        if (candidatos.Length == 0)
            return null;

        var motivo = candidatos[0];
        return new(
            motivo.NumeroEscova,
            motivo.Gate.NomeRegra ?? motivo.Gate.TipoRegra,
            motivo.Gate.TipoRegra,
            motivo.Gate.CodigoRegra,
            motivo.Gate.ValorObservado,
            motivo.Gate.ValorLimite,
            motivo.Gate.MensagemDiagnostico);
    }
}
