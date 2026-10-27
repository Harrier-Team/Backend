namespace Hackatohm.Application.MotorStatus;

public interface IMotorStatusService
{
    MotorStatusResult Agregar(
        int motorId,
        IReadOnlyList<MotorStatusReading> leituras,
        TimeSpan offlineAfter,
        DateTimeOffset agora,
        int? totalSensores = null);

    Task<MotorStatusResult?> ObterAsync(
        int motorId,
        TimeSpan offlineAfter,
        DateTimeOffset agora,
        CancellationToken cancellationToken = default);
}
