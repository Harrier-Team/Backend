using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;

namespace Hackatohm.Application.Abstractions.Persistence;

public interface ILeituraTemperaturaRepository
{
    Task AdicionarRangeAsync(
        IEnumerable<LeituraTemperatura> leituras,
        CancellationToken cancellationToken = default);
    Task<LeituraTemperatura?> ObterPorIdAsync(long leituraId, CancellationToken cancellationToken = default);
    Task<LeituraTemperatura?> ObterMaisRecentePorSensorAsync(
        string sensorId,
        CancellationToken cancellationToken = default);
    Task<LeituraTemperatura?> ObterAnteriorPorSensorAsync(
        string sensorId,
        DateTimeOffset instante,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeituraTemperatura>> ObterPorMotorNoInstanteAsync(
        int motorId,
        DateTimeOffset instante,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém, por sensor, até <paramref name="quantidade"/> leituras anteriores a <paramref name="antesDe"/>
    /// (ou todas, quando nulo), da mais antiga para a mais recente.
    /// </summary>
    Task<IReadOnlyDictionary<string, IReadOnlyList<AmostraTemperatura>>> ObterHistoricoRecentePorSensoresAsync(
        IReadOnlyCollection<string> sensorIds,
        DateTimeOffset? antesDe,
        int quantidade,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, LeituraTemperatura>> ObterMaisRecentesPorSensoresAsync(
        IReadOnlyCollection<string> sensorIds,
        CancellationToken cancellationToken = default);
}
