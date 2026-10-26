using FluentValidation;
using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.Sensores;

public sealed class PingSensorUseCase(
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork,
    IValidator<PingSensorRequestDto> validator)
{
    public async Task<bool> ExecutarAsync(
        PingSensorRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var sensor = await sensorRepository.ObterPorIdAsync(request.SensorId, cancellationToken);
        if (sensor is null)
            return false;

        if (request.IpAddress is not null)
            sensor.IpAtual = request.IpAddress;

        sensor.UltimaConexao = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
