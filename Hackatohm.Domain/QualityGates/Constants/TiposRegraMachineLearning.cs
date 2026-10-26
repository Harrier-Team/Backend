namespace Hackatohm.Domain.QualityGates.Constants;

/// <summary>
/// Identificadores padronizados para regras de qualidade e diagnóstico baseadas em modelos de Machine Learning.
/// </summary>
public static class TiposRegraMachineLearning
{
    /// <summary>
    /// Avalia a ocorrência de anomalias multidimensionais usando modelos estatísticos ou não-supervisionados (ex.: Isolation Forest ou Autoencoder).
    /// </summary>
    public const string DeteccaoAnomalia = "ml.deteccao_anomalia";

    /// <summary>
    /// Estima o tempo restante de vida útil (RUL - Remaining Useful Life) das escovas ou do equipamento.
    /// </summary>
    public const string EstimativaVidaUtil = "ml.estimativa_rul";
}
