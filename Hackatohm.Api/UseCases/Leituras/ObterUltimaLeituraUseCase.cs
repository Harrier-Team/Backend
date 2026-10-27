using FluentValidation;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Rules;

namespace Hackatohm.Api.UseCases.Leituras;

public sealed class ObterUltimaLeituraUseCase(
    ILeituraTemperaturaRepository leituraTemperaturaRepository,
    ISensorRepository sensorRepository,
    IQualityGateEvaluator qualityGateEvaluator,
    IValidator<ObterUltimaLeituraRequestDto> validator)
{
    public async Task<UltimaLeituraResponseDto?> ExecutarAsync(
        ObterUltimaLeituraRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var sensorId = Sensor.CriarId(request.EspMacAddress, request.NumeroEscova);
        var sensor = await sensorRepository.ObterPorIdAsync(sensorId, cancellationToken);
        if (sensor is null)
            return null;

        var leitura = await leituraTemperaturaRepository
            .ObterMaisRecentePorSensorAsync(sensorId, cancellationToken);

        if (leitura is null)
            return null;

        var leituraAnterior = await leituraTemperaturaRepository.ObterAnteriorPorSensorAsync(
            sensorId, leitura.DataHora, cancellationToken);
        var lote = await leituraTemperaturaRepository.ObterPorMotorNoInstanteAsync(
            sensor.MotorId, leitura.DataHora, cancellationToken);

        var historico = await leituraTemperaturaRepository.ObterHistoricoRecentePorSensoresAsync(
            [sensorId],
            leitura.DataHora,
            ParametrosPrevisaoTendencia.JanelaMaxima - 1,
            cancellationToken);

        var contexto = new ContextoAvaliacao(
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
            leituraAnterior?.ValorCelsius,
            leituraAnterior is null ? null : leitura.DataHora - leituraAnterior.DataHora,
            historico.GetValueOrDefault(sensorId));
        var resultados = await qualityGateEvaluator.AvaliarAsync(
            contexto,
            cancellationToken);

        return new UltimaLeituraResponseDto(
            leitura.SensorId,
            leitura.ValorCelsius,
            leitura.DataHora,
            resultados.Any(resultado => resultado.DisparouAlerta),
            sensor.MotorId,
            TelemetriaRealtimeMapper.MapearResultados(resultados));
    }
}
