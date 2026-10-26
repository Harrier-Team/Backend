using FluentValidation;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;

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

        var contexto = new ContextoAvaliacao(
            sensor,
            new LeituraTelemetriaDto
            {
                NumeroEscova = sensor.NumeroEscova,
                Temperatura = leitura.ValorCelsius
            },
            leitura.DataHora);
        var resultados = await qualityGateEvaluator.AvaliarAsync(
            contexto,
            cancellationToken);

        return new UltimaLeituraResponseDto(
            leitura.SensorId,
            leitura.ValorCelsius,
            leitura.DataHora,
            leitura.AlertaDesgaste,
            sensor.MotorId,
            TelemetriaRealtimeMapper.MapearResultados(resultados));
    }
}
