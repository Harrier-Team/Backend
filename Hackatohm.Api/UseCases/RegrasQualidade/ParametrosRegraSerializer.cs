using System.Text.Json;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.QualityGates.Constants;
using Hackatohm.Domain.QualityGates.Rules;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

/// <summary>
/// Converte os parâmetros da API para o JSON persistido de cada tipo de regra, e vice-versa.
/// </summary>
internal static class ParametrosRegraSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serializar(string tipoRegra, ParametrosLimiarTemperaturaDto parametros) =>
        tipoRegra == TiposRegraTemperatura.PrevisaoTendencia
            ? JsonSerializer.Serialize(
                new ParametrosPrevisaoTendencia(
                    parametros.Aviso,
                    parametros.Critico,
                    parametros.TemperaturaLimite
                        ?? throw new InvalidOperationException("A temperatura limite é obrigatória."),
                    parametros.Janela ?? ParametrosPrevisaoTendencia.JanelaPadrao),
                JsonOptions)
            : JsonSerializer.Serialize(
                new ParametrosLimiarTemperatura(parametros.Aviso, parametros.Critico),
                JsonOptions);

    public static ParametrosLimiarTemperaturaDto Desserializar(string tipoRegra, string parametrosJson, Guid regraId)
    {
        var invalido = new InvalidOperationException(
            $"A regra de qualidade {regraId} possui parâmetros inválidos.");

        if (tipoRegra == TiposRegraTemperatura.PrevisaoTendencia)
        {
            var tendencia = JsonSerializer.Deserialize<ParametrosPrevisaoTendencia>(parametrosJson, JsonOptions)
                ?? throw invalido;
            return new(tendencia.Aviso, tendencia.Critico, tendencia.TemperaturaLimite, tendencia.Janela);
        }

        var limiar = JsonSerializer.Deserialize<ParametrosLimiarTemperatura>(parametrosJson, JsonOptions)
            ?? throw invalido;
        return new(limiar.Aviso, limiar.Critico);
    }
}
