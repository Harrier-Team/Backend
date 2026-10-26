using Hackatohm.Api.DTOs;
using Hackatohm.Api.Errors;
using Hackatohm.Api.UseCases.RegrasQualidade;
using Microsoft.AspNetCore.Mvc;

namespace Hackatohm.Api.Controllers;

[ApiController]
[Route("api/regras-qualidade")]
public sealed class RegrasQualidadeController(
    CadastrarRegraQualidadeUseCase cadastrarUseCase,
    ListarRegrasQualidadeUseCase listarUseCase,
    ObterRegraQualidadeUseCase obterUseCase,
    EditarRegraQualidadeUseCase editarUseCase,
    ExcluirRegraQualidadeUseCase excluirUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RegraQualidadeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegraQualidadeResponseDto>> Cadastrar(
        [FromBody] CadastrarRegraQualidadeRequestDto request,
        CancellationToken cancellationToken)
    {
        var regra = await cadastrarUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = regra.Id }, regra);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RegraQualidadeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RegraQualidadeResponseDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var regras = await listarUseCase.ExecutarAsync(cancellationToken);
        return Ok(regras);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RegraQualidadeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegraQualidadeResponseDto>> Obter(
        Guid id,
        CancellationToken cancellationToken)
    {
        var regra = await obterUseCase.ExecutarAsync(id, cancellationToken);
        return regra is null
            ? ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.QualityRuleNotFound)
            : Ok(regra);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RegraQualidadeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegraQualidadeResponseDto>> Editar(
        Guid id,
        [FromBody] EditarRegraQualidadeRequestDto request,
        CancellationToken cancellationToken)
    {
        var regra = await editarUseCase.ExecutarAsync(id, request, cancellationToken);
        return regra is null
            ? ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.QualityRuleNotFound)
            : Ok(regra);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var excluida = await excluirUseCase.ExecutarAsync(id, cancellationToken);
        return excluida
            ? NoContent()
            : ApiErrorResponseFactory.ToProblemResult(HttpContext, ApiErrorCatalog.QualityRuleNotFound);
    }
}
