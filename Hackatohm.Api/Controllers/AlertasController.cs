using Hackatohm.Api.DTOs;
using Hackatohm.Api.UseCases.Alertas;
using Microsoft.AspNetCore.Mvc;

namespace Hackatohm.Api.Controllers;

[ApiController]
[Route("api/alertas")]
public sealed class AlertasController(ListarAlertasUseCase listarAlertasUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(ListaPaginadaResponseDto<AlertaManutencaoResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ListaPaginadaResponseDto<AlertaManutencaoResponseDto>>> ListarAlertas(
        [FromQuery] ListarAlertasRequestDto request,
        CancellationToken cancellationToken) =>
        Ok(await listarAlertasUseCase.ExecutarAsync(request, cancellationToken));
}
