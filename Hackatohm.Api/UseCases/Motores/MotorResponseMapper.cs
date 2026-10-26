using Hackatohm.Api.DTOs;
using Hackatohm.Domain.Entities;

namespace Hackatohm.Api.UseCases.Motores;

internal static class MotorResponseMapper
{
    public static MotorResponseDto Mapear(Motor motor) =>
        new(
            motor.Id,
            motor.Nome,
            motor.Tipo,
            [.. motor.Sensores.Select(sensor => new SensorResponseDto(
                sensor.Id,
                sensor.NumeroEscova,
                sensor.Modelo,
                sensor.IpAtual,
                sensor.UltimaConexao))]);
}
