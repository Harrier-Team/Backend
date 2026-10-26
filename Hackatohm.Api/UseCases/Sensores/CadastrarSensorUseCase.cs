using FluentValidation;
using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.Entities;

namespace Hackatohm.Api.UseCases.Sensores;

public enum StatusCadastroSensor
{
    Sucesso,
    MotorNaoEncontrado,
    SensorJaCadastrado
}

public sealed record CadastrarSensorResult(
    StatusCadastroSensor Status,
    SensorResponseDto? Sensor = null);

public sealed class CadastrarSensorUseCase(
    ISensorRepository sensorRepository,
    IMotorRepository motorRepository,
    IUnitOfWork unitOfWork,
    IValidator<CadastrarSensorRequestDto> validator)
{
    public async Task<CadastrarSensorResult> ExecutarAsync(
        CadastrarSensorRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        if (!await motorRepository.ExisteAsync(request.MotorId, cancellationToken))
            return new(StatusCadastroSensor.MotorNaoEncontrado);

        var sensorId = Sensor.CriarId(request.MacAddress, request.NumeroEscova);
        if (await sensorRepository.ExisteAsync(sensorId, cancellationToken))
            return new(StatusCadastroSensor.SensorJaCadastrado);

        var sensor = new Sensor
        {
            Id = sensorId,
            NumeroEscova = request.NumeroEscova,
            Modelo = request.Modelo.Trim(),
            MotorId = request.MotorId
        };

        await sensorRepository.AdicionarAsync(sensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new(
            StatusCadastroSensor.Sucesso,
            new SensorResponseDto(
                sensor.Id,
                sensor.NumeroEscova,
                sensor.Modelo,
                sensor.IpAtual,
                sensor.UltimaConexao));
    }
}
