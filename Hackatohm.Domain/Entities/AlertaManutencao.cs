using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Domain.Entities;

/// <summary>
/// Evento de alerta de manutenção: nasce quando um Quality Gate passa a disparar e é
/// encerrado quando ele deixa de disparar. Não guarda uma linha por leitura.
/// </summary>
public class AlertaManutencao
{
    public long Id { get; set; }
    public int MotorId { get; set; }
    public Motor Motor { get; set; } = null!;

    /// <summary>Sensor do alerta; nulo quando a regra descreve o motor inteiro (assimetria entre escovas).</summary>
    public string? SensorId { get; set; }
    public Sensor? Sensor { get; set; }
    public int? NumeroEscova { get; set; }

    /// <summary>Regra que originou o alerta; nulo se a regra foi excluída depois.</summary>
    public Guid? RegraQualidadeId { get; set; }
    public RegraQualidade? RegraQualidade { get; set; }
    public string NomeRegra { get; set; } = string.Empty;
    public string TipoRegra { get; set; } = string.Empty;
    public string CodigoRegra { get; set; } = string.Empty;

    /// <summary>Severidade mais recente do alerta enquanto aberto.</summary>
    public NivelSeveridade Severidade { get; set; }
    public double? ValorObservado { get; set; }
    public double? ValorLimite { get; set; }
    public string Mensagem { get; set; } = string.Empty;

    public DateTimeOffset IniciadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Preenchido quando o gate deixou de disparar; nulo enquanto o alerta está ativo.</summary>
    public DateTimeOffset? ResolvidoEm { get; set; }

    public bool Ativo => ResolvidoEm is null;
}
