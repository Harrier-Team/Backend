using FluentValidation;
using Hackatohm.Api.Data.Interfaces;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class ExcluirRegraQualidadeUseCase(
    IRegraQualidadeRepository regraQualidadeRepository,
    IUnitOfWork unitOfWork,
    ILogger<ExcluirRegraQualidadeUseCase> logger)
{
    public async Task<bool> ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var regra = await regraQualidadeRepository.ObterPorIdAsync(id, cancellationToken);
        if (regra is null)
            return false;

        regraQualidadeRepository.Excluir(regra);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Regra de qualidade {RegraId} excluída com seus vínculos.",
            id);
        return true;
    }
}