namespace Hackatohm.Domain.QualityGates.Constants;

/// <summary>
/// Identificadores padronizados para regras de qualidade e detecção de desgaste baseadas em termometria.
/// </summary>
public static class TiposRegraTemperatura
{
    /// <summary>
    /// Avalia se a temperatura instantânea ultrapassou limites estáticos pré-determinados (ex.: aviso e crítico).
    /// </summary>
    public const string LimiarEstatico = "temperatura.limiar_estatico";

    /// <summary>
    /// Avalia a taxa de variação da temperatura no tempo (dT/dt).
    /// Subidas abruptas indicam perda de contato mecânico ou centelhamento na escova.
    /// </summary>
    public const string GradienteTermico = "temperatura.gradiente_termico";

    /// <summary>
    /// Avalia a diferença térmica entre diferentes escovas do mesmo motor (T_max - T_min).
    /// Assimetrias térmicas indicam desgaste irregular ou sobrecarga pontual.
    /// </summary>
    public const string AssimetriaEscovas = "temperatura.assimetria_escovas";

    /// <summary>
    /// Estima, por regressão linear das últimas leituras, em quantos minutos a escova
    /// atingirá uma temperatura limite se a tendência atual se mantiver.
    /// </summary>
    public const string PrevisaoTendencia = "temperatura.previsao_tendencia";

    /// <summary>
    /// Avalia a previsão de desgaste em ml com base na temperatura e histórico de leituras.
    /// Sendo uma avaliação feita por machine learning, a implementação desta regra ainda não está disponível.
    /// </summary>
    public const string PrevisaoDesgasteMl = "temperatura.previsao_desgaste_ml";
}
