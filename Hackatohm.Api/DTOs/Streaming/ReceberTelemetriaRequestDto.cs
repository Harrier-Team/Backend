namespace Hackatohm.Api.DTOs;

public class ReceberTelemetriaRequestDto
{
    public string EspMacAddress { get; set; } = string.Empty;
    public List<LeituraTelemetriaDto> Sensores { get; set; } = [];
}