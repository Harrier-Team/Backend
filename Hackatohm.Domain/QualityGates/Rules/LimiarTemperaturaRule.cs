using System.Text.Json;
using Hackatohm.Domain.QualityGates.Constants;
using Hackatohm.Domain.QualityGates;

namespace Hackatohm.Domain.QualityGates.Rules;

/// <summary>
/// Parâmetros de configuração para a regra de limiar térmico estático.
/// </summary>
/// <param name="Aviso">Temperatura em °C que dispara alerta preditivo preventivo.</param>
/// <param name="Critico">Temperatura em °C que caracteriza condição crítica imediata.</param>
public record ParametrosLimiarTemperatura(double Aviso, double Critico);

/// <summary>
/// Regra que avalia se a temperatura instantânea da escova/sensor ultrapassa os limiares de aviso e crítico.
/// </summary>
public class LimiarTemperaturaRule : IQualityGateRule
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string TipoRegra => TiposRegraTemperatura.LimiarEstatico;

    public ValueTask<ResultadoGateQualidade> AvaliarAsync(ContextoAvaliacao contexto, string parametrosJson)
    {
        var config = JsonSerializer.Deserialize<ParametrosLimiarTemperatura>(parametrosJson, JsonOptions)
            ?? throw new InvalidOperationException("A configuração da regra de limiar está vazia.");

        if (!double.IsFinite(config.Aviso) ||
            !double.IsFinite(config.Critico) ||
            config.Aviso < -55 ||
            config.Aviso > 125 ||
            config.Critico < -55 ||
            config.Critico > 125 ||
            config.Aviso >= config.Critico)
        {
            throw new InvalidOperationException("A configuração da regra de limiar é inválida.");
        }

        var temperatura = contexto.LeituraAtual.Temperatura;

        if (temperatura >= config.Critico)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.Critico(
                TipoRegra,
                "TEMP_CRITICA",
                $"Escova {contexto.Sensor.NumeroEscova} atingiu temperatura crítica de {temperatura:F1} °C.",
                temperatura,
                config.Critico));
        }

        if (temperatura >= config.Aviso)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.AlertaPreditivo(
                TipoRegra,
                "TEMP_ALERTA_PREDITIVO",
                $"Temperatura elevada ({temperatura:F1} °C) acima da margem segura de aviso ({config.Aviso:F1} °C).",
                temperatura,
                config.Aviso));
        }

        return ValueTask.FromResult(ResultadoGateQualidade.Normal(TipoRegra));
    }
}