namespace Hackatohm.Api.DTOs.Hubs;

public record TelemetriaRealtimeDto(
    string SensorId,
    string EspMacAddress,
    int NumeroEscova,
    double ValorCelsius,
    DateTime DataHora,
    bool AlertaDesgaste,
    int? MotorId,
    IReadOnlyList<ResultadoGateRealtimeDto> ResultadosQualityGates
);