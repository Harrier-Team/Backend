using System.Text.Json;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates.Rules;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

internal static class RegraQualidadeResponseMapper
{
    public static RegraQualidadeResponseDto Mapear(RegraQualidade regra)
    {
        var parametros = ParametrosRegraSerializer.Desserializar(
            regra.TipoRegra, regra.ParametrosJson, regra.Id);

        return new(
            regra.Id,
            regra.Nome,
            regra.Descricao,
            regra.TipoRegra,
            regra.SeveridadePadrao,
            parametros,
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
