namespace Hackatohm.Api.DTOs;

public sealed class ObterUltimaLeituraRequestDto
{
    public string EspMacAddress { get; set; } = string.Empty;
    public int NumeroEscova { get; set; }
}
