using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hackatohm.Api.Data.Repositories;

public sealed class MotorRegraQualidadeRepository(AppDbContext context) : IMotorRegraQualidadeRepository
{
    public Task<MotorRegraQualidade?> ObterAsync(
        int motorId,
        Guid regraQualidadeId,
        CancellationToken cancellationToken = default) =>
        context.MotoresRegrasQualidade
            .Include(vinculo => vinculo.RegraQualidade)
            .FirstOrDefaultAsync(
                vinculo => vinculo.MotorId == motorId &&
                           vinculo.RegraQualidadeId == regraQualidadeId,
                cancellationToken);

    public async Task<IReadOnlyList<MotorRegraQualidade>> ListarPorMotorAsync(
        int motorId,
        CancellationToken cancellationToken = default) =>
        await context.MotoresRegrasQualidade
            .AsNoTracking()
            .Include(vinculo => vinculo.RegraQualidade)
            .Where(vinculo => vinculo.MotorId == motorId)
            .OrderBy(vinculo => vinculo.RegraQualidade.Nome)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(
        MotorRegraQualidade vinculo,
        CancellationToken cancellationToken = default) =>
        await context.MotoresRegrasQualidade.AddAsync(vinculo, cancellationToken);

    public void Remover(MotorRegraQualidade vinculo) =>
        context.MotoresRegrasQualidade.Remove(vinculo);
}
