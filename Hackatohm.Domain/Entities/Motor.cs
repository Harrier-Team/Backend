namespace Hackatohm.Domain.Entities;

/// <summary>
/// Representa um motor elétrico monitorado pelo sistema de telemetria preditiva.
/// </summary>
public class Motor
{
    public int Id { get; set; }

    /// <summary>
    /// Identificador legível ou tag do motor (ex.: "Motor CC - Linha 01").
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Tipo ou classificação do motor informada no cadastro (ex.: "Corrente Contínua", "Trifásico de Indução").
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Sensores físicos (ex.: ESP32 acoplados às escovas) instalados neste motor.
    /// </summary>
    public ICollection<Sensor> Sensores { get; set; } = [];

    /// <summary>
    /// Regras e gates de qualidade associados a este motor para monitoramento contínuo de desgaste.
    /// </summary>
    public ICollection<MotorRegraQualidade> RegrasAssociadas { get; set; } = [];
}
