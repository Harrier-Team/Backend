using Hackatohm.Api.Data;
using Hackatohm.Api.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hackatohm.Api.Observability;

public sealed class DatabaseHealthCheck(
    IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Não foi possível conectar ao PostgreSQL.");
    }
}

public sealed class TelemetriaQueueHealthCheck(
    TelemetriaQueue queue) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var snapshot = queue.GetSnapshot();
        var utilization = snapshot.Capacity == 0
            ? 1
            : (double)snapshot.ApproximateLength / snapshot.Capacity;

        var result = utilization >= 0.9
            ? HealthCheckResult.Degraded(
                "A fila de telemetria está próxima da capacidade máxima.")
            : HealthCheckResult.Healthy();

        return Task.FromResult(result);
    }
}
