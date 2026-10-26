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

        var contextos = sensores
            .Where(sensor => leituras.ContainsKey(sensor.Id))
            .Select(sensor =>
            {
                var leitura = leituras[sensor.Id];
                return new ContextoAvaliacao(
                    sensor,
                    new LeituraTelemetriaDto
                    {
                        NumeroEscova = sensor.NumeroEscova,
                        Temperatura = leitura.ValorCelsius
                    },
                    leitura.DataHora,
                    sensores
                        .Where(irma => leituras.ContainsKey(irma.Id))
                        .Select(irma => new LeituraTelemetriaDto
                        {
                            NumeroEscova = irma.NumeroEscova,
                            Temperatura = leituras[irma.Id].ValorCelsius
                        })
                        .ToArray());
            })
            .ToArray();

        var gates = await qualityGateEvaluator.AvaliarLoteAsync(contextos, cancellationToken);
        var leiturasStatus = contextos
            .Select(contexto => new MotorStatusReading(
                contexto.Sensor.Id,
                contexto.Sensor.NumeroEscova,
                leituras[contexto.Sensor.Id].ValorCelsius,
                leituras[contexto.Sensor.Id].DataHora,
                leituras[contexto.Sensor.Id].AlertaDesgaste,
                gates[contexto.Sensor.Id]))
            .ToArray();

        return Agregar(motorId, leiturasStatus, offlineAfter, agora);
    }

    public MotorStatusResult Agregar(
        int motorId,
        IReadOnlyList<MotorStatusReading> leituras,
        TimeSpan offlineAfter,
        DateTimeOffset agora)
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
        var situacao = escovas.Length == 0 || leiturasRecentes == 0
            ? SituacaoMotor.Offline
            : leiturasRecentes == escovas.Length
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
