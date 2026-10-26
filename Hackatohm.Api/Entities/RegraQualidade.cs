namespace Hackatohm.Api.Entities;

using Hackatohm.Api.QualityGates.Enums;

/// <summary>
/// Representa uma definição no catálogo de regras e gates de qualidade para monitoramento de equipamentos.
/// </summary>
/// <remarks>
/// Cada regra define os critérios de avaliação (limiares, gradientes, modelos de ML) e os parâmetros serializados em JSON,
/// permitindo que uma mesma política seja associada a múltiplos motores simultaneamente.
/// </remarks>
public class RegraQualidade
{
    public Guid Id { get; set; }

    /// <summary>
    /// Nome de exibição amigável para a regra (ex.: "Limiar de Atenção - Escovas 80°C").
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do objetivo ou contexto operacional da regra.
    /// </summary>
    public string? Descricao { get; set; }

    /// <summary>
    /// Identificador textual com namespace do tipo de regra (ex.: "temperatura.limiar_estatico", "ml.deteccao_anomalia").
    /// </summary>
    public string TipoRegra { get; set; } = string.Empty;

    /// <summary>
    /// Severidade padrão disparada caso os critérios desta regra sejam violados.
    /// </summary>
    public NivelSeveridade SeveridadePadrao { get; set; } = NivelSeveridade.AlertaPreditivo;

    /// <summary>
    /// Parâmetros de configuração específicos da regra armazenados em formato JSON (PostgreSQL JSONB).
    /// </summary>
    /// <remarks>
    /// Exemplos:
    /// - Limiar: {"aviso": 75.0, "critico": 90.0}
    /// - Gradiente: {"deltaCelsius": 5.0, "janelaSegundos": 60}
    /// - ML: {"modeloId": "v1.0", "thresholdConfianca": 0.85}
    /// </remarks>
    public string ParametrosJson { get; set; } = "{}";

    /// <summary>
    /// Indica se a regra está globalmente ativa no catálogo do sistema.
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Data e hora UTC em que a regra foi registrada no catálogo.
    /// </summary>
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Coleção de vínculos entre esta regra e os motores do sistema.
    /// </summary>
    public ICollection<MotorRegraQualidade> MotoresVinculados { get; set; } = [];
}
