namespace Hackatohm.Api.DTOs;

public record SensorInicialMotorRequestDto(
    string MacAddress, 
    int NumeroEscova, 
    string Modelo);
