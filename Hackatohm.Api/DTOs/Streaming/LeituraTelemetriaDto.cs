namespace Hackatohm.Api.DTOs;

/// <summary>Leitura de temperatura de uma escova no lote do dispositivo.</summary>
public sealed class LeituraTelemetriaDto
{
    public int NumeroEscova { get; set; }
    public double Temperatura { get; set; }
}