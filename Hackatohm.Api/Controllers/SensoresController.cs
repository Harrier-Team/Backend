using Hackatohm.Api.DTOs;
using Hackatohm.Api.Errors;
using Hackatohm.Api.UseCases.Sensores;
using Microsoft.AspNetCore.Mvc;

namespace Hackatohm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SensoresController(
    CadastrarSensorUseCase cadastrarSensorUseCase,
    ObterSensorUseCase obterSensorUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CadastrarSensor(
        [FromBody] CadastrarSensorRequestDto request,
        CancellationToken cancellationToken)
    {
        var resultado = await cadastrarSensorUseCase.ExecutarAsync(request, cancellationToken);

        return resultado.Status switch
        {
            StatusCadastroSensor.Sucesso =>
                CreatedAtAction(
                    nameof(ObterSensor),
                    new { sensorId = resultado.Sensor!.Id },
                    resultado.Sensor),
            StatusCadastroSensor.MotorNaoEncontrado =>
                ApiErrorResponseFactory.ToProblemResult(
                    HttpContext,
                    ApiErrorCatalog.MotorReferenceNotFound),
            StatusCadastroSensor.SensorJaCadastrado =>
                ApiErrorResponseFactory.ToProblemResult(
                    HttpContext,
                    ApiErrorCatalog.SensorAlreadyRegistered),
            _ => throw new InvalidOperationException(
                $"Status de cadastro de sensor não suportado: {resultado.Status}.")
        };
    }

    [HttpGet("{sensorId}")]
    public async Task<ActionResult<SensorResponseDto>> ObterSensor(
        string sensorId,
        CancellationToken cancellationToken)
    {
        var sensor = await obterSensorUseCase.ExecutarAsync(sensorId, cancellationToken);
        return sensor is null
            ? ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.SensorNotFound)
            : Ok(sensor);
    }
}
