using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class ListarRegrasQualidadeMotorUseCase(
    IMotorRepository motorRepository,
    IMotorRegraQualidadeRepository vinculoRepository)
{
    public async Task<IReadOnlyList<VinculoRegraQualidadeResponseDto>?> ExecutarAsync(
        int motorId,
        CancellationToken cancellationToken = default)
    {
        if (!await motorRepository.ExisteAsync(motorId, cancellationToken))
            return null;

        var vinculos = await vinculoRepository.ListarPorMotorAsync(motorId, cancellationToken);
        return vinculos.Select(RegraQualidadeResponseMapper.Mapear).ToArray();
    }
}
