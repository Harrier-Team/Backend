using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hackatohm.Infrastructure.Persistence.Repositories;

public sealed class AlertaManutencaoRepository(AppDbContext context) : IAlertaManutencaoRepository
{
    public async Task<IReadOnlyList<AlertaManutencao>> ObterAbertosPorMotorAsync(
        int motorId,
        CancellationToken cancellationToken = default) =>
        await context.AlertasManutencao
            .Where(alerta => alerta.MotorId == motorId && alerta.ResolvidoEm == null)
            .ToListAsync(cancellationToken);

    public Task AdicionarRangeAsync(
        IEnumerable<AlertaManutencao> alertas,
        CancellationToken cancellationToken = default) =>
        context.AlertasManutencao.AddRangeAsync(alertas, cancellationToken);

    public async Task<PaginaAlertas> ListarAsync(
        FiltroAlertas filtro,
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ArgumentOutOfRangeException.ThrowIfLessThan(pagina, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(tamanho, 1);

        var consulta = context.AlertasManutencao.AsNoTracking().AsQueryable();
        if (filtro.MotorId is { } motorId)
            consulta = consulta.Where(alerta => alerta.MotorId == motorId);
        if (filtro.Ativo is { } ativo)
            consulta = ativo
                ? consulta.Where(alerta => alerta.ResolvidoEm == null)
                : consulta.Where(alerta => alerta.ResolvidoEm != null);
        if (filtro.TiposRegra is { } tipos)
            consulta = consulta.Where(alerta => tipos.Contains(alerta.TipoRegra));
        if (filtro.Severidade is { } severidade)
            consulta = consulta.Where(alerta => alerta.Severidade == severidade);

        var total = await consulta.CountAsync(cancellationToken);
        var itens = await consulta
            .OrderByDescending(alerta => alerta.IniciadoEm)
            .ThenByDescending(alerta => alerta.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(cancellationToken);

        return new PaginaAlertas(itens, total);
    }
}
