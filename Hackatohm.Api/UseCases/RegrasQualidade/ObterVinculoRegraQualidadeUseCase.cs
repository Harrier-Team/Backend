using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class ObterVinculoRegraQualidadeUseCase(IMotorRegraQualidadeRepository repository)
{
    public async Task<VinculoRegraQualidadeResponseDto?> ExecutarAsync(
        int motorId,
        Guid regraId,
        CancellationToken cancellationToken = default)
    {
        var vinculo = await repository.ObterAsync(motorId, regraId, cancellationToken);
        return vinculo is null ? null : RegraQualidadeResponseMapper.Mapear(vinculo);
    }
}
