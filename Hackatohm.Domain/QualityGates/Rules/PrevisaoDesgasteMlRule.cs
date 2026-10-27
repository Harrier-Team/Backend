using System.Text.Json;
using Hackatohm.Domain.QualityGates.Constants;

namespace Hackatohm.Domain.QualityGates.Rules;

/// <summary>
/// Regra que avalia a previsão de desgaste em ml com base na temperatura e histórico de leituras.
/// Sendo uma avaliação feita por machine learning, a implementação desta regra ainda não está disponível.
/// </summary>
public sealed class PrevisaoDesgasteMlRule : IQualityGateRule
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string TipoRegra => TiposRegraTemperatura.PrevisaoDesgasteMl;

    public ValueTask<ResultadoGateQualidade> AvaliarAsync(
        ContextoAvaliacao contexto,
        string parametrosJson)
    {
        throw new NotImplementedException("A regra de previsão de desgaste em ml ainda não foi implementada.");
    }

    private static void ValidarConfiguracao(ParametrosLimiarTemperatura config)
    {
        
    }
}