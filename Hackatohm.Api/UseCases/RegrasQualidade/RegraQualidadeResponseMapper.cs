using System.Text.Json;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates.Rules;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

internal static class RegraQualidadeResponseMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static RegraQualidadeResponseDto Mapear(RegraQualidade regra)
    {
        var parametros = JsonSerializer.Deserialize<ParametrosLimiarTemperatura>(
            regra.ParametrosJson,
            JsonOptions) ?? throw new InvalidOperationException(
                $"A regra de qualidade {regra.Id} possui parâmetros inválidos.");

        return new(
            regra.Id,
            regra.Nome,
            regra.Descricao,
            regra.TipoRegra,
            regra.SeveridadePadrao,
            new(parametros.Aviso, parametros.Critico),
            regra.Ativo,
            regra.CriadoEm);
    }

    public static VinculoRegraQualidadeResponseDto Mapear(MotorRegraQualidade vinculo) =>
        new(
            vinculo.MotorId,
            vinculo.RegraQualidadeId,
            vinculo.Ativo,
            vinculo.VinculadoEm,
            Mapear(vinculo.RegraQualidade));
}
