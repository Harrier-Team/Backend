namespace Hackatohm.Api.Hubs;

using Hackatohm.Api.DTOs.Hubs;

public interface ITelemetriaClient
{
    Task ReceberTelemetria(TelemetriaRealtimeDto telemetria);
    Task ReceberLoteTelemetria(IEnumerable<TelemetriaRealtimeDto> leituras);
}