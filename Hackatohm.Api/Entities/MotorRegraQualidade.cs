namespace Hackatohm.Api.Entities;

/// <summary>
/// Tabela associativa (muitos-para-muitos) que vincula uma <see cref="RegraQualidade"/> a um <see cref="Motor"/>.
/// </summary>
/// <remarks>
/// Permite associar regras do catálogo a motores específicos e gerenciar a ativação individual de cada regra por equipamento.
/// </remarks>
public class MotorRegraQualidade
{
    public int MotorId { get; set; }
    public Motor Motor { get; set; } = null!;

    public Guid RegraQualidadeId { get; set; }
    public RegraQualidade RegraQualidade { get; set; } = null!;

    /// <summary>
    /// Permite suspender ou ativar temporariamente a avaliação desta regra para este motor específico.
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Data e hora UTC em que a regra foi vinculada ao motor.
    /// </summary>
    public DateTimeOffset VinculadoEm { get; set; } = DateTimeOffset.UtcNow;
}
