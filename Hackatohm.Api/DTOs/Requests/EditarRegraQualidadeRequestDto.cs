namespace Hackatohm.Api.DTOs;

/// <summary>Dados para editar uma regra de limiar de temperatura existente.</summary>
public sealed record EditarRegraQualidadeRequestDto : IRegraQualidadeRequestDto
{
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public required ParametrosLimiarTemperaturaDto Parametros { get; init; }
    public bool Ativo { get; init; }
}
