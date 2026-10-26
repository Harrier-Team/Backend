using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class ListarRegrasQualidadeUseCase(IRegraQualidadeRepository repository)
{
    public async Task<IReadOnlyList<RegraQualidadeResponseDto>> ExecutarAsync(
        CancellationToken cancellationToken = default)
    {
        var regras = await repository.ListarAsync(cancellationToken);
        return regras.Select(RegraQualidadeResponseMapper.Mapear).ToArray();
    }
}
