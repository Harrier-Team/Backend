using Hackatohm.Domain.Entities;

namespace Hackatohm.Application.Abstractions.Persistence;

public interface IRegraQualidadeRepository
{
    Task<RegraQualidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RegraQualidade>> ListarAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(RegraQualidade regra, CancellationToken cancellationToken = default);
    void Atualizar(RegraQualidade regra);
    void Excluir(RegraQualidade regra);
    Task<IReadOnlyDictionary<int, IReadOnlyList<RegraQualidade>>> ObterAtivasPorMotoresAsync(
        IReadOnlyCollection<int> motorIds,
        CancellationToken cancellationToken = default);
}