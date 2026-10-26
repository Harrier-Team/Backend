using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.Motores;

public sealed class ObterMotorUseCase(IMotorRepository motorRepository)
{
    public async Task<MotorResponseDto?> ExecutarAsync(
        int motorId,
        CancellationToken cancellationToken = default)
    {
        var motor = await motorRepository.ObterComSensoresAsync(motorId, cancellationToken);
        return motor is null ? null : MotorResponseMapper.Mapear(motor);
    }
}
