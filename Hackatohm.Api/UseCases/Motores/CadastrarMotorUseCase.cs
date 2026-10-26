using FluentValidation;
using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.Entities;

namespace Hackatohm.Api.UseCases.Motores;

public enum StatusCadastroMotor
{
    Sucesso,
    SensoresJaCadastrados
}

public sealed record CadastrarMotorResult(
    StatusCadastroMotor Status,
    MotorResponseDto? Motor = null,
    IReadOnlyList<string>? SensoresExistentes = null);

public sealed class CadastrarMotorUseCase(
    IMotorRepository motorRepository,
    IUnitOfWork unitOfWork,
    IValidator<CadastrarMotorRequestDto> validator)
{
    public async Task<CadastrarMotorResult> ExecutarAsync(
        CadastrarMotorRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var idsSensores = request.SensoresIniciais
            .Select(sensor => Sensor.CriarId(sensor.MacAddress, sensor.NumeroEscova))
            .ToArray();
        var sensoresExistentes = await motorRepository.ObterIdsSensoresExistentesAsync(
            idsSensores,
            cancellationToken);

        if (sensoresExistentes.Count > 0)
        {
            return new(
                StatusCadastroMotor.SensoresJaCadastrados,
                SensoresExistentes: sensoresExistentes);
        }

        var motor = new Motor
        {
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo.Trim(),
            Sensores = [.. request.SensoresIniciais.Select(sensor => new Sensor
            {
                Id = Sensor.CriarId(sensor.MacAddress, sensor.NumeroEscova),
                NumeroEscova = sensor.NumeroEscova,
                Modelo = sensor.Modelo.Trim()
            })]
        };

        await motorRepository.AdicionarAsync(motor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new(StatusCadastroMotor.Sucesso, MotorResponseMapper.Mapear(motor));
    }
}
