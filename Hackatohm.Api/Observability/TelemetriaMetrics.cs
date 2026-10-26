using System.Diagnostics.Metrics;
namespace Hackatohm.Api.Observability;

public sealed class TelemetriaMetrics : IDisposable
{
    private readonly Meter _meter = new("Hackatohm.Api.Telemetria");
    private readonly Counter<long> _accepted;
    private readonly Counter<long> _rejected;
    private readonly Counter<long> _processed;
    private readonly Counter<long> _failed;

    public TelemetriaMetrics()
    {
        _accepted = _meter.CreateCounter<long>("hackatohm.telemetria.accepted");
        _rejected = _meter.CreateCounter<long>("hackatohm.telemetria.rejected");
        _processed = _meter.CreateCounter<long>("hackatohm.telemetria.processed");
        _failed = _meter.CreateCounter<long>("hackatohm.telemetria.failed");
    }

    public void Accepted() => _accepted.Add(1);
    public void Rejected() => _rejected.Add(1);
    public void Processed() => _processed.Add(1);
    public void Failed() => _failed.Add(1);

    public void Dispose() => _meter.Dispose();
}
