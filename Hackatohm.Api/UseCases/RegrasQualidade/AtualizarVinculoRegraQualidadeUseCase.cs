using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class AtualizarVinculoRegraQualidadeUseCase(
    IMotorRegraQualidadeRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<VinculoRegraQualidadeResponseDto?> ExecutarAsync(
        int motorId,
        Guid regraId,
        AtualizarVinculoRegraQualidadeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var vinculo = await repository.ObterAsync(motorId, regraId, cancellationToken);
        if (vinculo is null)
            return null;

        vinculo.Ativo = request.Ativo;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return RegraQualidadeResponseMapper.Mapear(vinculo);
    }
}
