using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class ObterRegraQualidadeUseCase(IRegraQualidadeRepository repository)
{
    public async Task<RegraQualidadeResponseDto?> ExecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var regra = await repository.ObterPorIdAsync(id, cancellationToken);
        return regra is null ? null : RegraQualidadeResponseMapper.Mapear(regra);
    }
}
