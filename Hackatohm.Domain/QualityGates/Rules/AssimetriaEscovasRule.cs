using System.Text.Json;
using Hackatohm.Domain.QualityGates.Constants;

namespace Hackatohm.Domain.QualityGates.Rules;

/// <summary>
/// Avalia a diferença entre a maior e a menor temperatura das escovas no lote.
/// </summary>
public sealed class AssimetriaEscovasRule : IQualityGateRule
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string TipoRegra => TiposRegraTemperatura.AssimetriaEscovas;

    public ValueTask<ResultadoGateQualidade> AvaliarAsync(
        ContextoAvaliacao contexto,
        string parametrosJson)
    {
        var config = JsonSerializer.Deserialize<ParametrosLimiarTemperatura>(
            parametrosJson,
            JsonOptions) ?? throw new InvalidOperationException(
                "A configuração da regra de assimetria está vazia.");

        ValidarConfiguracao(config);

        var leituras = contexto.LeiturasIrmasDoLote?
            .Where(leitura => double.IsFinite(leitura.Temperatura))
            .ToArray() ?? [];

        if (leituras.Length < 2)
            return ValueTask.FromResult(ResultadoGateQualidade.Normal(TipoRegra));

        var temperaturaMinima = leituras.Min(leitura => leitura.Temperatura);
        var temperaturaMaxima = leituras.Max(leitura => leitura.Temperatura);
        var assimetria = temperaturaMaxima - temperaturaMinima;

        if (assimetria >= config.Critico)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.Critico(
                TipoRegra,
                "ASSIMETRIA_ESCOVAS_CRITICA",
                $"A diferença entre as escovas do motor atingiu {assimetria:F1} °C.",
                assimetria,
                config.Critico,
                new Dictionary<string, object>
                {
                    ["temperaturaMinima"] = temperaturaMinima,
                    ["temperaturaMaxima"] = temperaturaMaxima
                }));
        }

        if (assimetria >= config.Aviso)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.AlertaPreditivo(
                TipoRegra,
                "ASSIMETRIA_ESCOVAS_ALERTA",
                $"A diferença entre as escovas do motor atingiu {assimetria:F1} °C.",
                assimetria,
                config.Aviso,
                new Dictionary<string, object>
                {
                    ["temperaturaMinima"] = temperaturaMinima,
                    ["temperaturaMaxima"] = temperaturaMaxima
                }));
        }

        return ValueTask.FromResult(ResultadoGateQualidade.Normal(TipoRegra));
    }

    private static void ValidarConfiguracao(ParametrosLimiarTemperatura config)
    {
        if (!double.IsFinite(config.Aviso) ||
            !double.IsFinite(config.Critico) ||
            config.Aviso < 0 ||
            config.Critico < 0 ||
            config.Aviso >= config.Critico)
        {
            throw new InvalidOperationException(
                "A configuração da regra de assimetria deve conter limiares finitos positivos, com aviso menor que crítico.");
        }
    }
}