using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.UseCases.Motores;

public sealed class ListarMotoresUseCase(IMotorRepository motorRepository)
{
    public async Task<IReadOnlyList<MotorResponseDto>> ExecutarAsync(
        CancellationToken cancellationToken = default)
    {
        var motores = await motorRepository.ListarComSensoresAsync(cancellationToken);
        return [.. motores.Select(MotorResponseMapper.Mapear)];
    }
}
