namespace Hackatohm.Api.DTOs;

/// <summary>Representa a leitura mais recente de um sensor.</summary>
public sealed record UltimaLeituraResponseDto(
    string SensorId,
    double ValorCelsius,
    DateTimeOffset DataHora,
    bool AlertaDesgaste);
