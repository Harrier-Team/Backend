using Hackatohm.Api.QualityGates.Models;

namespace Hackatohm.Api.QualityGates;

/// <summary>
/// Avalia as regras de qualidade ativas associadas ao motor de um sensor.
/// </summary>
public interface IQualityGateEvaluator
{
    /// <summary>
    /// Executa todas as regras configuradas para o motor do sensor informado.
    /// </summary>
    /// <param name="contexto">Leitura atual e dados temporais relacionados.</param>
    /// <param name="cancellationToken">Token para cancelar a consulta e a avaliação.</param>
    /// <returns>Resultados de todas as regras ativas associadas ao motor.</returns>
    Task<IReadOnlyList<ResultadoGateQualidade>> AvaliarAsync(
        ContextoAvaliacao contexto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Avalia vários sensores usando uma única consulta para carregar as regras dos motores envolvidos.
    /// </summary>
    Task<IReadOnlyDictionary<string, IReadOnlyList<ResultadoGateQualidade>>> AvaliarLoteAsync(
        IReadOnlyList<ContextoAvaliacao> contextos,
        CancellationToken cancellationToken = default);
}