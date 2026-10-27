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
                var resultado = await useCase.ProcessarAsync(msg, stoppingToken);
                if (resultado.Status == StatusProcessamentoLote.Sucesso)
                    queue.MarkProcessed();
                else
                    queue.MarkFailed();
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
