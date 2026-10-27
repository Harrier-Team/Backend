using FluentValidation;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.QualityGates.Rules;
using System.Text.Json;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class EditarRegraQualidadeUseCase(
    IRegraQualidadeRepository regraQualidadeRepository,
    IUnitOfWork unitOfWork,
    IValidator<EditarRegraQualidadeRequestDto> validator,
    ILogger<EditarRegraQualidadeUseCase> logger)
{
    public async Task<RegraQualidadeResponseDto?> ExecutarAsync(
        Guid id,
        EditarRegraQualidadeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var regra = await regraQualidadeRepository.ObterPorIdAsync(id, cancellationToken);
        if (regra is null)
            return null;

        regra.Nome = request.Nome.Trim();
        regra.Descricao = request.Descricao?.Trim();
        regra.TipoRegra = request.TipoRegra;
        regra.ParametrosJson = ParametrosRegraSerializer.Serializar(request.TipoRegra, request.Parametros);
        regra.Ativo = request.Ativo;

        regraQualidadeRepository.Atualizar(regra);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Regra de qualidade {RegraId} editada. Ativo: {Ativo}.",
            regra.Id,
            regra.Ativo);
        return RegraQualidadeResponseMapper.Mapear(regra);
    }
}