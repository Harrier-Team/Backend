using FluentValidation;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.Sensores;

public sealed class ObterSensorUseCase(
    ISensorRepository sensorRepository,
    IValidator<ObterSensorRequestDto> validator)
{
    public async Task<SensorResponseDto?> ExecutarAsync(
        string sensorId,
        CancellationToken cancellationToken = default)
    {
        var request = new ObterSensorRequestDto(sensorId);
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var sensor = await sensorRepository.ObterPorIdAsync(sensorId, cancellationToken);
        return sensor is null
            ? null
            : new SensorResponseDto(
                sensor.Id,
                sensor.NumeroEscova,
                sensor.Modelo,
                sensor.IpAtual,
                sensor.UltimaConexao);
    }
}
