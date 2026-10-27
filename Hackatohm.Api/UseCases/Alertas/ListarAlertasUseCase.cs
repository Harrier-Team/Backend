using FluentValidation;
using Hackatohm.Api.DTOs;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.QualityGates;

namespace Hackatohm.Api.UseCases.Alertas;

public sealed class ListarAlertasUseCase(
    IAlertaManutencaoRepository alertaRepository,
    IValidator<ListarAlertasRequestDto> validator)
{
    public async Task<ListaPaginadaResponseDto<AlertaManutencaoResponseDto>> ExecutarAsync(
        ListarAlertasRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var filtro = new FiltroAlertas(
            request.MotorId,
            request.Ativo,
            TiposRegra(request),
            request.Severidade);

        var pagina = await alertaRepository.ListarAsync(
            filtro, request.Pagina, request.Tamanho, cancellationToken);

        return new(
            [.. pagina.Itens.Select(AlertaManutencaoMapper.Mapear)],
            request.Pagina,
            request.Tamanho,
            pagina.Total);
    }

    private static IReadOnlyCollection<string>? TiposRegra(ListarAlertasRequestDto request)
    {
        var tipoRegra = string.IsNullOrWhiteSpace(request.TipoRegra) ? null : request.TipoRegra.Trim();
        if (request.Categoria is not { } categoria)
            return tipoRegra is null ? null : [tipoRegra];

        var tiposDaCategoria = categoria.TiposRegra();
        return tipoRegra is null
            ? tiposDaCategoria
            : [.. tiposDaCategoria.Where(tipo => tipo == tipoRegra)];
    }
}
