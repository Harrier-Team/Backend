using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;

namespace Hackatohm.Application.Alertas;

/// <summary>Resultados dos gates de um sensor em um lote.</summary>
public sealed record AvaliacaoSensor(
    Sensor Sensor,
    IReadOnlyList<ResultadoGateQualidade> Resultados);

public interface IAlertaManutencaoService
{
    /// <summary>
    /// Compara os gates do lote com os alertas abertos do motor: abre os novos, atualiza os
    /// que continuam e encerra os que deixaram de disparar. Não salva; o chamador confirma
    /// a transação.
    /// </summary>
    /// <returns>Alertas recém-abertos ou cuja severidade subiu.</returns>
    Task<IReadOnlyList<AlertaManutencao>> RegistrarAsync(
        int motorId,
        IReadOnlyList<AvaliacaoSensor> avaliacoes,
        DateTimeOffset agora,
        CancellationToken cancellationToken = default);
}
