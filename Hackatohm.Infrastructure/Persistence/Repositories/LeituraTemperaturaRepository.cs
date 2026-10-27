using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hackatohm.Infrastructure.Persistence.Repositories;

public sealed class LeituraTemperaturaRepository(AppDbContext context) : ILeituraTemperaturaRepository
{
    public async Task AdicionarRangeAsync(
        IEnumerable<LeituraTemperatura> leituras,
        CancellationToken cancellationToken = default)
    {
        await context.LeiturasTemperatura.AddRangeAsync(leituras, cancellationToken);
    }

    public Task<LeituraTemperatura?> ObterPorIdAsync(
        long leituraId,
        CancellationToken cancellationToken = default)
    {
        return context.LeiturasTemperatura
            .AsNoTracking()
            .FirstOrDefaultAsync(leitura => leitura.Id == leituraId, cancellationToken);
    }

    public Task<LeituraTemperatura?> ObterMaisRecentePorSensorAsync(
        string sensorId,
        CancellationToken cancellationToken = default)
    {
        return context.LeiturasTemperatura
            .AsNoTracking()
            .Where(leitura => leitura.SensorId == sensorId)
            .OrderByDescending(leitura => leitura.DataHora)
            .ThenByDescending(leitura => leitura.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<LeituraTemperatura?> ObterAnteriorPorSensorAsync(
        string sensorId,
        DateTimeOffset instante,
        CancellationToken cancellationToken = default) =>
        context.LeiturasTemperatura
            .AsNoTracking()
            .Where(leitura => leitura.SensorId == sensorId && leitura.DataHora < instante)
            .OrderByDescending(leitura => leitura.DataHora)
            .ThenByDescending(leitura => leitura.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<LeituraTemperatura>> ObterPorMotorNoInstanteAsync(
        int motorId,
        DateTimeOffset instante,
        CancellationToken cancellationToken = default) =>
        await context.LeiturasTemperatura
            .AsNoTracking()
            .Include(leitura => leitura.Sensor)
            .Where(leitura => leitura.Sensor.MotorId == motorId && leitura.DataHora == instante)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<string, LeituraTemperatura>> ObterMaisRecentesPorSensoresAsync(
        IReadOnlyCollection<string> sensorIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sensorIds);
        if (sensorIds.Count == 0)
            return new Dictionary<string, LeituraTemperatura>(StringComparer.OrdinalIgnoreCase);

        var leituras = await context.LeiturasTemperatura
            .AsNoTracking()
            .Where(leitura => sensorIds.Contains(leitura.SensorId))
            .GroupBy(leitura => leitura.SensorId)
            .Select(grupo => grupo
                .OrderByDescending(leitura => leitura.DataHora)
                .ThenByDescending(leitura => leitura.Id)
                .First())
            .ToListAsync(cancellationToken);

        return leituras.ToDictionary(
            leitura => leitura.SensorId,
            StringComparer.OrdinalIgnoreCase);
    }
}
