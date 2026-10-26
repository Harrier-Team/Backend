using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Hackatohm.Api.Queues;

public sealed class TelemetriaQueue
{
    private readonly Channel<TelemetriaRecebida> _channel;
    private readonly int _capacity;
    private long _acceptedCount;
    private long _rejectedCount;
    private long _processedCount;
    private long _failedCount;

    public TelemetriaQueue(IOptions<TelemetriaOptions> options)
    {
        var settings = options.Value;
        if (settings.QueueCapacity <= 0)
            throw new InvalidOperationException("Telemetria:QueueCapacity deve ser positivo.");

        _capacity = settings.QueueCapacity;
        _channel = Channel.CreateBounded<TelemetriaRecebida>(
            new BoundedChannelOptions(_capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public bool TryEnqueue(TelemetriaRecebida telemetria)
    {
        var accepted = _channel.Writer.TryWrite(telemetria);
        if (accepted)
            Interlocked.Increment(ref _acceptedCount);
        else
            Interlocked.Increment(ref _rejectedCount);

        return accepted;
    }

    public void MarkProcessed() => Interlocked.Increment(ref _processedCount);

    public void MarkFailed() => Interlocked.Increment(ref _failedCount);

    public TelemetriaQueueSnapshot GetSnapshot() =>
        new(
            _capacity,
            _channel.Reader.Count,
            Interlocked.Read(ref _acceptedCount),
            Interlocked.Read(ref _rejectedCount),
            Interlocked.Read(ref _processedCount),
            Interlocked.Read(ref _failedCount));

    public IAsyncEnumerable<TelemetriaRecebida> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}