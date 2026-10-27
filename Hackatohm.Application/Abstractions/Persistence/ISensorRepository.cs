using Hackatohm.Domain.Entities;

namespace Hackatohm.Application.Abstractions.Persistence;

public interface ISensorRepository
{
    Task AdicionarAsync(Sensor sensor, CancellationToken cancellationToken = default);
    Task<Sensor?> ObterPorIdAsync(string sensorId, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(string sensorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sensor>> ObterPorMotorAsync(
        int motorId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, Sensor>> ObterPorIdsComoDicionarioAsync(
        IReadOnlyCollection<string> sensorIds,
        CancellationToken cancellationToken = default);
}
