namespace Hackatohm.Domain.Entities;

// Representa um sensor de temperatura instalado em um motor específico.
public class Sensor
{
    // O ID pode ser o MAC Address do ESP32 ou um Guid, 
    // facilitando a identificação.
    public string Id { get; set; } = string.Empty;
    public int NumeroEscova { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string? IpAtual { get; set; }
    public DateTimeOffset? UltimaConexao { get; set; }
    
    // Chave Estrangeira
    public int MotorId { get; set; }
    public Motor Motor { get; set; } = null!;

    public static string CriarId(string macAddress, int numeroEscova) =>
        $"{macAddress.Trim().ToUpperInvariant()}_Escova{numeroEscova}";
}