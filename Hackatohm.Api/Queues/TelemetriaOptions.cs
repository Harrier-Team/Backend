namespace Hackatohm.Api.Queues;

public sealed class TelemetriaOptions
{
    public const string SectionName = "Telemetria";

    public int QueueCapacity { get; set; } = 5_000;
    public int MaxBatchSize { get; set; } = 100;
}

public sealed record TelemetriaQueueSnapshot(
    int Capacity,
    int ApproximateLength,
    long AcceptedCount,
    long RejectedCount,
    long ProcessedCount,
    long FailedCount);
