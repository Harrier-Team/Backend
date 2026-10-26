namespace Hackatohm.Api.DTOs;

/// <summary>Payload enviado por um dispositivo com as leituras do lote.</summary>
public sealed class ReceberTelemetriaRequestDto
{
    public string EspMacAddress { get; set; } = string.Empty;
    public List<LeituraTelemetriaDto> Sensores { get; set; } = [];
}