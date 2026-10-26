namespace Hackatohm.Api.DTOs;

public record SensorResponseDto(
    string Id,
    int NumeroEscova,
    string Modelo,
    string? IpAtual,
    DateTime? UltimaConexao);
