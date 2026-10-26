namespace Hackatohm.Api.DTOs.Hubs;

/// <summary>Representa uma leitura de telemetria publicada em tempo real.</summary>
public sealed record TelemetriaRealtimeDto(
    string SensorId,
    string EspMacAddress,
    int NumeroEscova,
    double ValorCelsius,
    DateTimeOffset DataHora,
    bool AlertaDesgaste,
    int? MotorId,
    IReadOnlyList<ResultadoGateRealtimeDto> ResultadosQualityGates
);