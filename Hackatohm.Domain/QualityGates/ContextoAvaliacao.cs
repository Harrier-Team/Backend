using Hackatohm.Domain.Entities;

namespace Hackatohm.Domain.QualityGates;

/// <summary>
/// Encapsula todos os dados operacionais necessários para a avaliação de um Quality Gate.
/// </summary>
/// <param name="Sensor">Entidade do sensor que originou a telemetria (contém número da escova e motor vinculado).</param>
/// <param name="LeituraAtual">Dado instantâneo recebido no pacote da telemetria.</param>
/// <param name="DataHora">Timestamp UTC de recebimento do pacote.</param>
/// <param name="LeiturasIrmasDoLote">Outras leituras recebidas no mesmo pacote (permite calcular desbalanceamento/assimetria entre escovas).</param>
/// <param name="TemperaturaAnterior">Última temperatura persistida do sensor (permite calcular gradiente térmico dT/dt).</param>
/// <param name="IntervaloDesdeUltimaLeitura">Tempo decorrido desde a última telemetria deste sensor.</param>
public record ContextoAvaliacao(
    Sensor Sensor,
    LeituraTelemetriaDto LeituraAtual,
    DateTimeOffset DataHora,
    IReadOnlyList<LeituraTelemetriaDto>? LeiturasIrmasDoLote = null,
    double? TemperaturaAnterior = null,
    TimeSpan? IntervaloDesdeUltimaLeitura = null
);
