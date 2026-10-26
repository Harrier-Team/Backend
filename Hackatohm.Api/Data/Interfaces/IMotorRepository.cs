using Hackatohm.Api.Entities;

namespace Hackatohm.Api.Data.Interfaces;

public interface IMotorRepository
{
    Task AdicionarAsync(Motor motor, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(int motorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ObterIdsSensoresExistentesAsync(
        IReadOnlyCollection<string> sensorIds,
        CancellationToken cancellationToken = default);
    Task<Motor?> ObterComSensoresAsync(int motorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Motor>> ListarComSensoresAsync(CancellationToken cancellationToken = default);
    Task<Motor?> ObterParaRemocaoAsync(int motorId, CancellationToken cancellationToken = default);
    void Remover(Motor motor);
}
