using Hackatohm.Domain.Entities;

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
    Task<IReadOnlyDictionary<string, LeituraTemperatura>> ObterMaisRecentesPorSensoresAsync(
        IReadOnlyCollection<string> sensorIds,
        CancellationToken cancellationToken = default);
}
