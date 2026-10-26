using FluentValidation;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.Entities;

namespace Hackatohm.Api.UseCases.Leituras;

public sealed class ObterUltimaLeituraUseCase(
    ILeituraTemperaturaRepository leituraTemperaturaRepository,
    IValidator<ObterUltimaLeituraRequestDto> validator)
{
    public async Task<UltimaLeituraResponseDto?> ExecutarAsync(
        ObterUltimaLeituraRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var sensorId = Sensor.CriarId(request.EspMacAddress, request.NumeroEscova);
        var leitura = await leituraTemperaturaRepository
            .ObterMaisRecentePorSensorAsync(sensorId, cancellationToken);

        return leitura is null
            ? null
            : new UltimaLeituraResponseDto(
                leitura.SensorId,
                leitura.ValorCelsius,
                leitura.DataHora,
                leitura.AlertaDesgaste);
    }
}
