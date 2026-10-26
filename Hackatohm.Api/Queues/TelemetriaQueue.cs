using System.Threading.Channels;

namespace Hackatohm.Api.Queues;

public sealed class TelemetriaQueue
{
    private readonly Channel<TelemetriaRecebida> _channel =
        Channel.CreateBounded<TelemetriaRecebida>(new BoundedChannelOptions(5_000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    public bool TryEnqueue(TelemetriaRecebida telemetria) => _channel.Writer.TryWrite(telemetria);
    public IAsyncEnumerable<TelemetriaRecebida> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}