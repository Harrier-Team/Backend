using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class AtualizarVinculoRegraQualidadeUseCase(
    IMotorRegraQualidadeRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<AtualizarVinculoRegraQualidadeUseCase> logger)
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
        logger.LogInformation(
            "Vínculo da regra {RegraId} no motor {MotorId} atualizado. Ativo: {Ativo}.",
            regraId,
            motorId,
            vinculo.Ativo);
        return RegraQualidadeResponseMapper.Mapear(vinculo);
    }
}
