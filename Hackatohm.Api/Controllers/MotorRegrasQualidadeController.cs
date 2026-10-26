using Hackatohm.Api.DTOs;
using Hackatohm.Api.Errors;
using Hackatohm.Api.UseCases.RegrasQualidade;
using Microsoft.AspNetCore.Mvc;

namespace Hackatohm.Api.Controllers;

[ApiController]
[Route("api/motores/{motorId:int}/regras-qualidade")]
public sealed class MotorRegrasQualidadeController(
    VincularRegraQualidadeMotorUseCase vincularUseCase,
    ListarRegrasQualidadeMotorUseCase listarUseCase,
    ObterVinculoRegraQualidadeUseCase obterUseCase,
    AtualizarVinculoRegraQualidadeUseCase atualizarUseCase,
    DesvincularRegraQualidadeMotorUseCase desvincularUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VinculoRegraQualidadeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<VinculoRegraQualidadeResponseDto>>> Listar(
        int motorId,
        CancellationToken cancellationToken)
    {
        var vinculos = await listarUseCase.ExecutarAsync(motorId, cancellationToken);
        return vinculos is null
            ? ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.MotorNotFound)
            : Ok(vinculos);
    }

    [HttpGet("{regraId:guid}")]
    [ProducesResponseType(typeof(VinculoRegraQualidadeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VinculoRegraQualidadeResponseDto>> Obter(
        int motorId,
        Guid regraId,
        CancellationToken cancellationToken)
    {
        var vinculo = await obterUseCase.ExecutarAsync(motorId, regraId, cancellationToken);
        return vinculo is null
            ? ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.QualityRuleLinkNotFound)
            : Ok(vinculo);
    }

    [HttpPost("{regraId:guid}")]
    [ProducesResponseType(typeof(VinculoRegraQualidadeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VinculoRegraQualidadeResponseDto>> Vincular(
        int motorId,
        Guid regraId,
        [FromBody] CriarVinculoRegraQualidadeRequestDto request,
        CancellationToken cancellationToken)
    {
        var resultado = await vincularUseCase.ExecutarAsync(
            motorId,
            regraId,
            request,
            cancellationToken);

        return resultado.Status switch
        {
            StatusVinculoRegraQualidade.Criado =>
                CreatedAtAction(nameof(Obter), new { motorId, regraId }, resultado.Vinculo),
            StatusVinculoRegraQualidade.MotorNaoEncontrado =>
                ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.MotorNotFound),
            StatusVinculoRegraQualidade.RegraNaoEncontrada =>
                ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.QualityRuleNotFound),
            StatusVinculoRegraQualidade.JaVinculada =>
                ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.QualityRuleAlreadyLinked),
            _ => throw new InvalidOperationException($"Status de vínculo não suportado: {resultado.Status}.")
        };
    }

    [HttpPut("{regraId:guid}")]
    [ProducesResponseType(typeof(VinculoRegraQualidadeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VinculoRegraQualidadeResponseDto>> Atualizar(
        int motorId,
        Guid regraId,
        [FromBody] AtualizarVinculoRegraQualidadeRequestDto request,
        CancellationToken cancellationToken)
    {
        var vinculo = await atualizarUseCase.ExecutarAsync(
            motorId,
            regraId,
            request,
            cancellationToken);
        return vinculo is null
            ? ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.QualityRuleLinkNotFound)
            : Ok(vinculo);
    }

    [HttpDelete("{regraId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desvincular(
        int motorId,
        Guid regraId,
        CancellationToken cancellationToken)
    {
        var removido = await desvincularUseCase.ExecutarAsync(motorId, regraId, cancellationToken);
        return removido
            ? NoContent()
            : ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.QualityRuleLinkNotFound);
    }
}
