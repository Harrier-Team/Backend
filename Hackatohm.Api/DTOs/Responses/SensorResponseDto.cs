namespace Hackatohm.Api.DTOs;

/// <summary>Representa um sensor retornado pela API.</summary>
public sealed record SensorResponseDto(
    string Id,
    int NumeroEscova,
    string Modelo,
    string? IpAtual,
    DateTimeOffset? UltimaConexao);
