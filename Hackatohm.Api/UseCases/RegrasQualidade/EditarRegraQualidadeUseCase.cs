using FluentValidation;
using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.QualityGates.Rules;
using System.Text.Json;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class EditarRegraQualidadeUseCase(
    IRegraQualidadeRepository regraQualidadeRepository,
    IUnitOfWork unitOfWork,
    IValidator<EditarRegraQualidadeRequestDto> validator)
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
        regra.ParametrosJson = JsonSerializer.Serialize(
            new ParametrosLimiarTemperatura(request.Parametros.Aviso, request.Parametros.Critico),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        regra.Ativo = request.Ativo;

        regraQualidadeRepository.Atualizar(regra);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return RegraQualidadeResponseMapper.Mapear(regra);
    }
}