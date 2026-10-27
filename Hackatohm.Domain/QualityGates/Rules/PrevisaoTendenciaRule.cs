using System.Text.Json;
using Hackatohm.Domain.QualityGates.Constants;

namespace Hackatohm.Domain.QualityGates.Rules;

/// <summary>
/// Parâmetros da regra de previsão por tendência.
/// </summary>
/// <param name="Aviso">Antecedência em minutos que caracteriza alerta preditivo.</param>
/// <param name="Critico">Antecedência em minutos que caracteriza condição crítica; menor que o aviso.</param>
/// <param name="TemperaturaLimite">Temperatura em °C cuja chegada se quer antecipar.</param>
/// <param name="Janela">Quantidade máxima de leituras recentes usadas no ajuste, incluindo a atual.</param>
public record ParametrosPrevisaoTendencia(
    double Aviso,
    double Critico,
    double TemperaturaLimite,
    int Janela = ParametrosPrevisaoTendencia.JanelaPadrao)
{
    public const int JanelaPadrao = 12;
    public const int JanelaMinima = 5;
    public const int JanelaMaxima = 60;
}

/// <summary>
/// Ajusta uma reta às últimas leituras do sensor e estima em quantos minutos a
/// temperatura atingirá o limite configurado se a tendência se mantiver.
/// </summary>
public sealed class PrevisaoTendenciaRule : IQualityGateRule
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string TipoRegra => TiposRegraTemperatura.PrevisaoTendencia;

    public ValueTask<ResultadoGateQualidade> AvaliarAsync(
        ContextoAvaliacao contexto,
        string parametrosJson)
    {
        var config = JsonSerializer.Deserialize<ParametrosPrevisaoTendencia>(
            parametrosJson,
            JsonOptions) ?? throw new InvalidOperationException(
                "A configuração da regra de previsão por tendência está vazia.");

        ValidarConfiguracao(config);

        var amostras = (contexto.HistoricoRecente ?? [])
            .Where(amostra => amostra.DataHora < contexto.DataHora &&
                              double.IsFinite(amostra.Temperatura))
            .OrderBy(amostra => amostra.DataHora)
            .TakeLast(config.Janela - 1)
            .Append(new AmostraTemperatura(contexto.DataHora, contexto.LeituraAtual.Temperatura))
            .ToArray();

        if (amostras.Length < ParametrosPrevisaoTendencia.JanelaMinima)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.DadosInsuficientes(
                TipoRegra,
                $"Aguardando pelo menos {ParametrosPrevisaoTendencia.JanelaMinima} leituras para estimar a tendência."));
        }

        var origem = amostras[^1].DataHora;
        var tempos = amostras.Select(amostra => (amostra.DataHora - origem).TotalMinutes).ToArray();
        var temperaturas = amostras.Select(amostra => amostra.Temperatura).ToArray();
        var tempoMedio = tempos.Average();
        var temperaturaMedia = temperaturas.Average();
        var variacaoTempo = tempos.Sum(tempo => (tempo - tempoMedio) * (tempo - tempoMedio));

        if (variacaoTempo <= 0)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.DadosInsuficientes(
                TipoRegra, "As leituras recentes não cobrem um intervalo de tempo positivo."));
        }

        var inclinacao = tempos.Zip(temperaturas, (tempo, temperatura) =>
            (tempo - tempoMedio) * (temperatura - temperaturaMedia)).Sum() / variacaoTempo;

        if (inclinacao <= 0)
            return ValueTask.FromResult(ResultadoGateQualidade.Normal(TipoRegra));

        // Valor da reta no instante atual: reduz o efeito do ruído de uma única leitura.
        var temperaturaEstimada = temperaturaMedia - inclinacao * tempoMedio;
        var minutosAteLimite = Math.Max(0, (config.TemperaturaLimite - temperaturaEstimada) / inclinacao);

        var metadados = new Dictionary<string, object>
        {
            ["inclinacaoCelsiusPorMinuto"] = inclinacao,
            ["temperaturaLimite"] = config.TemperaturaLimite,
            ["amostras"] = amostras.Length
        };
        var mensagem =
            $"A escova {contexto.Sensor.NumeroEscova} tende a atingir {config.TemperaturaLimite:F1} °C " +
            $"em ~{minutosAteLimite:F0} min (+{inclinacao:F2} °C/min).";

        if (minutosAteLimite <= config.Critico)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.Critico(
                TipoRegra, "PREVISAO_TENDENCIA_CRITICA", mensagem,
                minutosAteLimite, config.Critico, metadados));
        }

        if (minutosAteLimite <= config.Aviso)
        {
            return ValueTask.FromResult(ResultadoGateQualidade.AlertaPreditivo(
                TipoRegra, "PREVISAO_TENDENCIA_ALERTA", mensagem,
                minutosAteLimite, config.Aviso, metadados));
        }

        return ValueTask.FromResult(ResultadoGateQualidade.Normal(TipoRegra));
    }

    private static void ValidarConfiguracao(ParametrosPrevisaoTendencia config)
    {
        if (!double.IsFinite(config.Aviso) ||
            !double.IsFinite(config.Critico) ||
            !double.IsFinite(config.TemperaturaLimite) ||
            config.Critico <= 0 ||
            config.Aviso <= config.Critico ||
            config.TemperaturaLimite < -55 ||
            config.TemperaturaLimite > 200 ||
            config.Janela < ParametrosPrevisaoTendencia.JanelaMinima ||
            config.Janela > ParametrosPrevisaoTendencia.JanelaMaxima)
        {
            throw new InvalidOperationException(
                "A configuração da regra de previsão por tendência é inválida: use minutos positivos com aviso maior que crítico, limite entre -55 e 200 °C e janela de 5 a 60 leituras.");
        }
    }
}
