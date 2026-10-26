namespace Hackatohm.Api.DTOs;

public sealed record UltimaLeituraResponseDto(
    string SensorId,
    double ValorCelsius,
    DateTime DataHora,
    bool AlertaDesgaste);
