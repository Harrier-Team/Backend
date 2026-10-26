using Hackatohm.Api.QualityGates.Models;

namespace Hackatohm.Api.QualityGates;

/// <summary>
/// Contrato para implementação de estratégias de avaliação de qualidade e desgaste (Quality Gates).
/// </summary>
/// <remarks>
/// Cada implementação é responsável por interpretar seus parâmetros JSON específicos,
/// analisar o contexto da telemetria e retornar um diagnóstico estruturado.
/// </remarks>
public interface IQualityGateRule
{
    /// <summary>
    /// Identificador textual único da regra (ex.: constantes em <c>TiposRegraTemperatura</c>).
    /// Utilizado pelo avaliador para localizar a estratégia correspondente.
    /// </summary>
    string TipoRegra { get; }

    /// <summary>
    /// Executa a avaliação dos dados de telemetria contra os critérios da regra.
    /// </summary>
    /// <param name="contexto">Dados da leitura atual, sensor, equipamento e dados temporais associados.</param>
    /// <param name="parametrosJson">Configuração da regra cadastrada no banco de dados.</param>
    /// <returns>Resultado da avaliação com severidade, diagnóstico legível e eventuais métricas calculadas.</returns>
    ValueTask<ResultadoGateQualidade> AvaliarAsync(ContextoAvaliacao contexto, string parametrosJson);
}