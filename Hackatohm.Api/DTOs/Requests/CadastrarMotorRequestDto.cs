namespace Hackatohm.Api.DTOs;

public record CadastrarMotorRequestDto(
    string Nome,
    string Tipo,
    List<SensorInicialMotorRequestDto> SensoresIniciais);
