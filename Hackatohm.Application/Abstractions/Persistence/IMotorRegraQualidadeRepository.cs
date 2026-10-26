using Hackatohm.Domain.Entities;

namespace Hackatohm.Application.Abstractions.Persistence;

public interface IMotorRegraQualidadeRepository
{
    Task<MotorRegraQualidade?> ObterAsync(
        int motorId,
        Guid regraQualidadeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MotorRegraQualidade>> ListarPorMotorAsync(
        int motorId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        MotorRegraQualidade vinculo,
        CancellationToken cancellationToken = default);

    void Remover(MotorRegraQualidade vinculo);
}
