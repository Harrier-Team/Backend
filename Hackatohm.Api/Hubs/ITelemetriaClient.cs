namespace Hackatohm.Api.Hubs;

using Hackatohm.Api.DTOs;
using Hackatohm.Api.DTOs.Hubs;
using Hackatohm.Api.DTOs.MotorStatus;

public interface ITelemetriaClient
{
    Task ReceberTelemetria(TelemetriaRealtimeDto telemetria);
    Task ReceberLoteTelemetria(IEnumerable<TelemetriaRealtimeDto> leituras);
    Task ReceberStatusMotor(MotorStatusResponseDto status);
    Task ReceberAlertaManutencao(AlertaManutencaoResponseDto alerta);
}