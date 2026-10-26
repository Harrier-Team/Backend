using Hackatohm.Api.Queues;
using Hackatohm.Api.UseCases.Leituras;

namespace Hackatohm.Api.Workers;

public sealed class TelemetriaWorker(
    TelemetriaQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<TelemetriaWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider
                    .GetRequiredService<ProcessarLeiturasLoteUseCase>();
                await useCase.ProcessarAsync(msg, stoppingToken);
                queue.MarkProcessed();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                queue.MarkFailed();
                logger.LogError(ex,
                    "Falha ao processar telemetria {RequestId} do ESP {Mac}.",
                    msg.RequestId,
                    msg.Payload.EspMacAddress);
            }
        }
    }
}