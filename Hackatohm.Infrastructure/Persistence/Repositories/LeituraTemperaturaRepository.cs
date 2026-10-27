using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;
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

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<AmostraTemperatura>>> ObterHistoricoRecentePorSensoresAsync(
        IReadOnlyCollection<string> sensorIds,
        DateTimeOffset? antesDe,
        int quantidade,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sensorIds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantidade);

        var historico = new Dictionary<string, IReadOnlyList<AmostraTemperatura>>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var sensorId in sensorIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var consulta = context.LeiturasTemperatura
                .AsNoTracking()
                .Where(leitura => leitura.SensorId == sensorId);
            if (antesDe is not null)
                consulta = consulta.Where(leitura => leitura.DataHora < antesDe);

            var leituras = await consulta
                .OrderByDescending(leitura => leitura.DataHora)
                .ThenByDescending(leitura => leitura.Id)
                .Take(quantidade)
                .Select(leitura => new AmostraTemperatura(leitura.DataHora, leitura.ValorCelsius))
                .ToListAsync(cancellationToken);
            leituras.Reverse();
            historico[sensorId] = leituras;
        }

        return historico;
    }

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
