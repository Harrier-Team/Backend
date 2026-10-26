using Hackatohm.Api.DTOs;
using Hackatohm.Api.Queues;
using Hackatohm.Api.Observability;
using Hackatohm.Api.Validation;
using Microsoft.Extensions.Options;

namespace Hackatohm.Api.UnitTests;

public sealed class TelemetriaIngestionTests
{
    [Fact]
    public async Task Validator_RejeitaLoteAcimaDoLimiteConfigurado()
    {
        var validator = new ReceberTelemetriaRequestDtoValidator(
            Options.Create(new TelemetriaOptions { MaxBatchSize = 1 }));
        var payload = new ReceberTelemetriaRequestDto
        {
            EspMacAddress = "AA:BB:CC:DD:EE:FF",
            Sensores =
            [
                new() { NumeroEscova = 1, Temperatura = 80 },
                new() { NumeroEscova = 2, Temperatura = 81 }
            ]
        };

        var result = await validator.ValidateAsync(payload);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Sensores");
    }

    [Fact]
    public void Queue_RecusaMensagensAlemDaCapacidadeERegistraContadores()
    {
        var queue = new TelemetriaQueue(
            Options.Create(new TelemetriaOptions { QueueCapacity = 1 }),
            new TelemetriaMetrics());
        var first = CriarTelemetria("request-1");
        var second = CriarTelemetria("request-2");

        Assert.True(queue.TryEnqueue(first));
        Assert.False(queue.TryEnqueue(second));
        queue.MarkProcessed();
        queue.MarkFailed();

        var snapshot = queue.GetSnapshot();

        Assert.Equal(1, snapshot.Capacity);
        Assert.Equal(1, snapshot.ApproximateLength);
        Assert.Equal(1, snapshot.AcceptedCount);
        Assert.Equal(1, snapshot.RejectedCount);
        Assert.Equal(1, snapshot.ProcessedCount);
        Assert.Equal(1, snapshot.FailedCount);
    }

    private static TelemetriaRecebida CriarTelemetria(string requestId) =>
        new(
            new ReceberTelemetriaRequestDto
            {
                EspMacAddress = "AA:BB:CC:DD:EE:FF",
                Sensores = [new() { NumeroEscova = 1, Temperatura = 80 }]
            },
            null,
            DateTimeOffset.UtcNow,
            requestId);
}
