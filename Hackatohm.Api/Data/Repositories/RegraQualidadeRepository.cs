using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hackatohm.Api.Data.Repositories;

public sealed class RegraQualidadeRepository(AppDbContext context) : IRegraQualidadeRepository
{
    public Task<RegraQualidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.RegrasQualidade.FirstOrDefaultAsync(regra => regra.Id == id, cancellationToken);

    public async Task<IReadOnlyList<RegraQualidade>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.RegrasQualidade
            .AsNoTracking()
            .OrderBy(regra => regra.Nome)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(RegraQualidade regra, CancellationToken cancellationToken = default) =>
        await context.RegrasQualidade.AddAsync(regra, cancellationToken);

    public void Atualizar(RegraQualidade regra) => context.RegrasQualidade.Update(regra);

    public void Excluir(RegraQualidade regra) => context.RegrasQualidade.Remove(regra);

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<RegraQualidade>>> ObterAtivasPorMotoresAsync(
        IReadOnlyCollection<int> motorIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(motorIds);
        if (motorIds.Count == 0)
            return new Dictionary<int, IReadOnlyList<RegraQualidade>>();

        var regrasVinculadas = await context.MotoresRegrasQualidade
            .AsNoTracking()
            .Where(vinculo =>
                motorIds.Contains(vinculo.MotorId) &&
                vinculo.Ativo &&
                vinculo.RegraQualidade.Ativo)
            .Select(vinculo => new
            {
                vinculo.MotorId,
                Regra = vinculo.RegraQualidade
            })
            .ToListAsync(cancellationToken);

        return regrasVinculadas
            .GroupBy(vinculo => vinculo.MotorId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => (IReadOnlyList<RegraQualidade>)grupo
                    .Select(vinculo => vinculo.Regra)
                    .ToList());
    }
}
