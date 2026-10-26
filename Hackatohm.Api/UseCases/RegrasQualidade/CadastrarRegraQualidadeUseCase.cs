using FluentValidation;
using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.Entities;
using Hackatohm.Api.QualityGates.Constants;
using Hackatohm.Api.QualityGates.Rules;
using System.Text.Json;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public sealed class CadastrarRegraQualidadeUseCase(
    IRegraQualidadeRepository regraQualidadeRepository,
    IUnitOfWork unitOfWork,
    IValidator<CadastrarRegraQualidadeRequestDto> validator)
{
    public async Task<RegraQualidadeResponseDto> ExecutarAsync(
        CadastrarRegraQualidadeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var regra = new RegraQualidade
        {
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim(),
            TipoRegra = TiposRegraTemperatura.LimiarEstatico,
            ParametrosJson = JsonSerializer.Serialize(
                new ParametrosLimiarTemperatura(request.Parametros.Aviso, request.Parametros.Critico),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            Ativo = request.Ativo
        };

        await regraQualidadeRepository.AdicionarAsync(regra, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return RegraQualidadeResponseMapper.Mapear(regra);
    }
}