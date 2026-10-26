using Hackatohm.Application.MotorStatus;
using Hackatohm.Api.DTOs.MotorStatus;
using Microsoft.Extensions.Options;
using Hackatohm.Api.Queues;

namespace Hackatohm.Api.UseCases.Motores;

public sealed class ObterStatusMotorUseCase(
    IMotorStatusService motorStatusService,
    IOptions<TelemetriaOptions> options)
{
    public async Task<MotorStatusResponseDto?> ExecutarAsync(
        int motorId,
        CancellationToken cancellationToken = default)
    {
        var result = await motorStatusService.ObterAsync(
            motorId,
            TimeSpan.FromSeconds(options.Value.OfflineAfterSeconds),
            DateTimeOffset.UtcNow,
            cancellationToken);

        return result is null ? null : MotorStatusResponseMapper.Mapear(result);
    }
}
