namespace Hackatohm.Api.Entities;

public class LeituraTemperatura
{
    public long Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public Sensor Sensor { get; set; } = null!;
    
    public double ValorCelsius { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public bool AlertaDesgaste { get; set; } // Flag para facilitar filtros no frontend
}