using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hackatohm.Api.Data.Repositories;

public class SensorRepository(AppDbContext context) : ISensorRepository
{
    public async Task AdicionarAsync(Sensor sensor, CancellationToken cancellationToken = default)
    {
        await context.Sensores.AddAsync(sensor, cancellationToken);
    }

    public async Task<Sensor?> ObterPorIdAsync(string sensorId, CancellationToken cancellationToken = default)
    {
        return await context.Sensores.FirstOrDefaultAsync(s => s.Id == sensorId, cancellationToken);
    }

    public Task<bool> ExisteAsync(string sensorId, CancellationToken cancellationToken = default) =>
        context.Sensores.AnyAsync(sensor => sensor.Id == sensorId, cancellationToken);

    public async Task<IReadOnlyDictionary<string, Sensor>> ObterPorIdsComoDicionarioAsync(
        IReadOnlyCollection<string> sensorIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sensorIds);
        if (sensorIds.Count == 0)
            return new Dictionary<string, Sensor>(StringComparer.OrdinalIgnoreCase);

        var sensores = await context.Sensores
            .Where(sensor => sensorIds.Contains(sensor.Id))
            .ToListAsync(cancellationToken);

        return sensores.ToDictionary(
            sensor => sensor.Id,
            StringComparer.OrdinalIgnoreCase);
    }
}