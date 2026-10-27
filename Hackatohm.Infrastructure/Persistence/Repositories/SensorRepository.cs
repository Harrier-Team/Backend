using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hackatohm.Infrastructure.Persistence.Repositories;

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

    public async Task<IReadOnlyList<Sensor>> ObterPorMotorAsync(
        int motorId,
        CancellationToken cancellationToken = default) =>
        await context.Sensores
            .Where(sensor => sensor.MotorId == motorId)
            .ToListAsync(cancellationToken);

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
