namespace Hackatohm.Api.DTOs;

using Hackatohm.Domain.QualityGates.Constants;

/// <summary>Dados para criar uma regra de limiar de temperatura.</summary>
public sealed record CadastrarRegraQualidadeRequestDto : IRegraQualidadeRequestDto
{
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public required ParametrosLimiarTemperaturaDto Parametros { get; init; }
    public string TipoRegra { get; init; } = TiposRegraTemperatura.LimiarEstatico;
    public bool Ativo { get; init; } = true;
}

/// <summary>Limites térmicos que acionam os estados de alerta e crítico.</summary>
public sealed record ParametrosLimiarTemperaturaDto(double Aviso, double Critico);

public interface IRegraQualidadeRequestDto
{
    string Nome { get; }
    string? Descricao { get; }
    ParametrosLimiarTemperaturaDto Parametros { get; }
    string TipoRegra { get; }
}
