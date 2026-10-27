using System.Text.Json;
using Hackatohm.Domain.QualityGates.Constants;

namespace Hackatohm.Domain.QualityGates.Rules;

/// <summary>
/// Avalia a velocidade de aumento da temperatura da escova em °C por minuto.
/// </summary>
public sealed class GradienteTemperaturaRule : IQualityGateRule
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string TipoRegra => TiposRegraTemperatura.GradienteTermico;

    public ValueTask<ResultadoGateQualidade> AvaliarAsync(
        ContextoAvaliacao contexto,
        string parametrosJson)
    {
        var config = JsonSerializer.Deserialize<ParametrosLimiarTemperatura>(
            parametrosJson,
            JsonOptions) ?? throw new InvalidOperationException(
                "A configuração da regra de gradiente está vazia.");

        ValidarConfiguracao(config);

        if (contexto.TemperaturaAnterior is null ||
            contexto.IntervaloDesdeUltimaLeitura is null ||
            contexto.IntervaloDesdeUltimaLeitura <= TimeSpan.Zero)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.DadosInsuficientes(
                TipoRegra, "Aguardando duas amostras com intervalo positivo para calcular o gradiente térmico."));
        }

        var gradiente = (contexto.LeituraAtual.Temperatura - contexto.TemperaturaAnterior.Value) /
            contexto.IntervaloDesdeUltimaLeitura.Value.TotalMinutes;

        if (gradiente <= 0)
            return ValueTask.FromResult(ResultadoGateQualidade.Normal(TipoRegra));

        if (gradiente >= config.Critico)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.Critico(
                TipoRegra,
                "GRADIENTE_TERMICO_CRITICO",
                $"A temperatura da escova {contexto.Sensor.NumeroEscova} está subindo a {gradiente:F2} °C/min.",
                gradiente,
                config.Critico,
                new Dictionary<string, object>
                {
                    ["temperaturaAnterior"] = contexto.TemperaturaAnterior.Value,
                    ["intervaloSegundos"] = contexto.IntervaloDesdeUltimaLeitura.Value.TotalSeconds
                }));
        }

        if (gradiente >= config.Aviso)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.AlertaPreditivo(
                TipoRegra,
                "GRADIENTE_TERMICO_ALERTA",
                $"A temperatura da escova {contexto.Sensor.NumeroEscova} está subindo a {gradiente:F2} °C/min.",
                gradiente,
                config.Aviso,
                new Dictionary<string, object>
                {
                    ["temperaturaAnterior"] = contexto.TemperaturaAnterior.Value,
                    ["intervaloSegundos"] = contexto.IntervaloDesdeUltimaLeitura.Value.TotalSeconds
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
                "A configuração da regra de gradiente deve conter limiares finitos positivos, com aviso menor que crítico.");
        }
    }
}
