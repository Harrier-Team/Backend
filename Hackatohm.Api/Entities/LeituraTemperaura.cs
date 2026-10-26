namespace Hackatohm.Api.Entities;

public class LeituraTemperatura
{
    public long Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public Sensor Sensor { get; set; } = null!;
    
    public double ValorCelsius { get; set; }
    public DateTimeOffset DataHora { get; set; } = DateTimeOffset.UtcNow;
    public bool AlertaDesgaste { get; set; } // Flag para facilitar filtros no frontend
}