using Hackatohm.Api.Data.Interfaces;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class DesvincularRegraQualidadeMotorUseCase(
    IMotorRegraQualidadeRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<DesvincularRegraQualidadeMotorUseCase> logger)
{
    public async Task<bool> ExecutarAsync(
        int motorId,
        Guid regraId,
        CancellationToken cancellationToken = default)
    {
        var vinculo = await repository.ObterAsync(motorId, regraId, cancellationToken);
        if (vinculo is null)
            return false;

        repository.Remover(vinculo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Regra {RegraId} desvinculada do motor {MotorId}.",
            regraId,
            motorId);
        return true;
    }
}
