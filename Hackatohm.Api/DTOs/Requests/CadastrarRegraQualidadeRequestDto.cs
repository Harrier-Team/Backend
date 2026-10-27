namespace Hackatohm.Api.DTOs;

using System.Text.Json.Serialization;
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

/// <summary>
/// Limites que acionam os estados de alerta e crítico. Na regra de previsão por tendência,
/// <c>aviso</c> e <c>critico</c> são minutos de antecedência (aviso maior que crítico) e
/// <c>temperaturaLimite</c> é obrigatória; <c>janela</c> é opcional (padrão 12).
/// </summary>
public sealed record ParametrosLimiarTemperaturaDto(
    double Aviso,
    double Critico,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? TemperaturaLimite = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? Janela = null);

public interface IRegraQualidadeRequestDto
{
    string Nome { get; }
    string? Descricao { get; }
    ParametrosLimiarTemperaturaDto Parametros { get; }
    string TipoRegra { get; }
}
