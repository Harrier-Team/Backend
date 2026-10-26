using Hackatohm.Api.DTOs;
using Hackatohm.Api.Errors;
using Hackatohm.Api.UseCases.Motores;
using Hackatohm.Api.UseCases.Sensores;
using Microsoft.AspNetCore.Mvc;

namespace Hackatohm.Api.Controllers;

[ApiController]
[Route("api/motores")]
public sealed class MotoresController(
    CadastrarMotorUseCase cadastrarMotorUseCase,
    ObterMotorUseCase obterMotorUseCase,
    ListarMotoresUseCase listarMotoresUseCase,
    DeletarMotorUseCase deletarMotorUseCase,
    PingSensorUseCase pingSensorUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MotorResponseDto>> CadastrarMotor(
        [FromBody] CadastrarMotorRequestDto request,
        CancellationToken cancellationToken)
    {
        var resultado = await cadastrarMotorUseCase.ExecutarAsync(request, cancellationToken);
        if (resultado.Status == StatusCadastroMotor.SensoresJaCadastrados)
        {
            var problem = ApiErrorResponseFactory.CreateProblemDetails(
                HttpContext,
                ApiErrorCatalog.SensorsAlreadyRegistered);
            problem.Detail =
                $"Os seguintes sensores já estão cadastrados no sistema: " +
                $"{string.Join(", ", resultado.SensoresExistentes!)}";
            return new ObjectResult(problem)
            {
                StatusCode = ApiErrorCatalog.SensorsAlreadyRegistered.StatusCode
            };
        }

        return CreatedAtAction(
            nameof(ObterMotor),
            new { id = resultado.Motor!.Id },
            resultado.Motor);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MotorResponseDto>> ObterMotor(
        int id,
        CancellationToken cancellationToken)
    {
        var motor = await obterMotorUseCase.ExecutarAsync(id, cancellationToken);
        return motor is null
            ? ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.MotorNotFound)
            : Ok(motor);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MotorResponseDto>>> ListarMotores(
        CancellationToken cancellationToken)
    {
        var motores = await listarMotoresUseCase.ExecutarAsync(cancellationToken);
        return Ok(motores);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletarMotor(
        int id,
        CancellationToken cancellationToken)
    {
        var removido = await deletarMotorUseCase.ExecutarAsync(id, cancellationToken);
        return removido
            ? NoContent()
            : ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.MotorNotFound);
    }

    [HttpPost("sensores/{sensorId}/ping")]
    public async Task<IActionResult> PingSensor(
        string sensorId,
        CancellationToken cancellationToken)
    {
        var enderecoIp = HttpContext.Connection.RemoteIpAddress;
        if (enderecoIp?.IsIPv4MappedToIPv6 == true)
            enderecoIp = enderecoIp.MapToIPv4();

        var atualizado = await pingSensorUseCase.ExecutarAsync(
            new PingSensorRequestDto(sensorId, enderecoIp?.ToString()),
            cancellationToken);

        return atualizado
            ? Ok()
            : ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.SensorNotFound);
    }
}
