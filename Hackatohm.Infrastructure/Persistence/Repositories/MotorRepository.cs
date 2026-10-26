using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hackatohm.Infrastructure.Persistence.Repositories;

public sealed class MotorRepository(AppDbContext context) : IMotorRepository
{
    public async Task AdicionarAsync(Motor motor, CancellationToken cancellationToken = default)
    {
        await context.Motores.AddAsync(motor, cancellationToken);
    }

    public Task<bool> ExisteAsync(int motorId, CancellationToken cancellationToken = default) =>
        context.Motores.AnyAsync(motor => motor.Id == motorId, cancellationToken);

    public async Task<IReadOnlyList<string>> ObterIdsSensoresExistentesAsync(
        IReadOnlyCollection<string> sensorIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sensorIds);
        if (sensorIds.Count == 0)
            return [];

        return await context.Sensores
            .AsNoTracking()
            .Where(sensor => sensorIds.Contains(sensor.Id))
            .Select(sensor => sensor.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Motor?> ObterComSensoresAsync(
        int motorId,
        CancellationToken cancellationToken = default) =>
        context.Motores
            .AsNoTracking()
            .Include(motor => motor.Sensores)
            .FirstOrDefaultAsync(motor => motor.Id == motorId, cancellationToken);

    public async Task<IReadOnlyList<Motor>> ListarComSensoresAsync(
        CancellationToken cancellationToken = default) =>
        await context.Motores
            .AsNoTracking()
            .Include(motor => motor.Sensores)
            .ToListAsync(cancellationToken);

    public Task<Motor?> ObterParaRemocaoAsync(
        int motorId,
        CancellationToken cancellationToken = default) =>
        context.Motores.FirstOrDefaultAsync(motor => motor.Id == motorId, cancellationToken);

    public void Remover(Motor motor) => context.Motores.Remove(motor);
}
