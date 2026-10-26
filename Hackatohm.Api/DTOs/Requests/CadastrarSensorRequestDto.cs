namespace Hackatohm.Api.DTOs;

public record CadastrarSensorRequestDto(
    string MacAddress,
    int NumeroEscova,
    string Modelo,
    int MotorId);
