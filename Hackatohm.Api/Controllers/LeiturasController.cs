using Hackatohm.Api.DTOs;
using Hackatohm.Api.Errors;
using Hackatohm.Api.UseCases.Leituras;
using Microsoft.AspNetCore.Mvc;

namespace Hackatohm.Api.Controllers;

[ApiController]
[Route("api/leituras")]
public class LeiturasController(
    ObterUltimaLeituraUseCase obterUltimaLeituraUseCase,
    ReceberTelemetriaUseCase receberTelemetriaUseCase) : ControllerBase
{
    [HttpPost]
    [ApiErrorAudience(ApiClientAudience.Device)]
    [ProducesResponseType(typeof(DispositivoSucessoResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ReceberLeituraEmLote(
        [FromBody] ReceberTelemetriaRequestDto? payload,
        CancellationToken cancellationToken)
    {
        if (payload is null)
        {
            return StatusCode(
                ApiErrorCatalog.InvalidPayload.StatusCode,
                ApiErrorResponseFactory.CreateDeviceError(
                    HttpContext,
                    ApiErrorCatalog.InvalidPayload));
        }

        var ipAtual = ObterEnderecoIpCliente();
        var resultado = await receberTelemetriaUseCase.ExecutarAsync(
            payload,
            ipAtual,
            HttpContext.TraceIdentifier,
            cancellationToken);

        return resultado switch
        {
            StatusRecebimentoTelemetria.Aceita =>
                Accepted(new DispositivoSucessoResponseDto(Ok: true)),
            StatusRecebimentoTelemetria.FilaCheia =>
                ResponderFilaCheia(),
            _ => throw new InvalidOperationException(
                $"Status de recebimento de telemetria não suportado: {resultado}.")
        };
    }

    [HttpGet]
    public async Task<IActionResult> ObterLeituraMaisRecente(
        [FromQuery] ObterUltimaLeituraRequestDto request,
        CancellationToken cancellationToken)
    {
        var leitura = await obterUltimaLeituraUseCase.ExecutarAsync(request, cancellationToken);
        if (leitura is null)
            return ApiErrorResponseFactory.ToProblemResult(
                HttpContext,
                ApiErrorCatalog.ReadingNotFound);

        return Ok(leitura);
    }

    private ObjectResult ResponderErroDispositivo(ApiErrorDefinition erro) =>
        StatusCode(
            erro.StatusCode,
            ApiErrorResponseFactory.CreateDeviceError(HttpContext, erro));

    private ObjectResult ResponderFilaCheia()
    {
        Response.Headers.RetryAfter = "5";
        return ResponderErroDispositivo(ApiErrorCatalog.ServiceUnavailable);
    }

    private string? ObterEnderecoIpCliente()
    {
        var enderecoIp = HttpContext.Connection.RemoteIpAddress;
        if (enderecoIp?.IsIPv4MappedToIPv6 == true)
            enderecoIp = enderecoIp.MapToIPv4();

        return enderecoIp?.ToString();
    }
}
