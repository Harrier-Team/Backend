namespace Hackatohm.Domain.QualityGates;

/// <summary>Ponto histórico de temperatura de um sensor.</summary>
/// <param name="DataHora">Instante da leitura.</param>
/// <param name="Temperatura">Temperatura em °C.</param>
public record AmostraTemperatura(DateTimeOffset DataHora, double Temperatura);
