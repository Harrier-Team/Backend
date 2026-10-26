using Hackatohm.Api.Data.Interfaces;

namespace Hackatohm.Api.UseCases.Motores;

public sealed class DeletarMotorUseCase(
    IMotorRepository motorRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<bool> ExecutarAsync(
        int motorId,
        CancellationToken cancellationToken = default)
    {
        var motor = await motorRepository.ObterParaRemocaoAsync(motorId, cancellationToken);
        if (motor is null)
            return false;

        motorRepository.Remover(motor);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
