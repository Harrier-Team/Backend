namespace Hackatohm.Api.DTOs;

public record MotorResponseDto(
    int Id,
    string Nome,
    string Tipo,
    List<SensorResponseDto> Sensores);
