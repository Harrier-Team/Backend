using FluentValidation;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.Queues;

namespace Hackatohm.Api.UseCases.Leituras;

public enum StatusRecebimentoTelemetria
{
    Aceita,
    FilaCheia
}

public sealed class ReceberTelemetriaUseCase(
    IValidator<ReceberTelemetriaRequestDto> validator,
    TelemetriaQueue queue,
    ILogger<ReceberTelemetriaUseCase> logger)
{
    public async Task<StatusRecebimentoTelemetria> ExecutarAsync(
        ReceberTelemetriaRequestDto payload,
        string? ipAtual,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        var dataRecebimento = DateTimeOffset.UtcNow;
        await validator.ValidateAndThrowAsync(payload, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var payloadSnapshot = new ReceberTelemetriaRequestDto
        {
            EspMacAddress = payload.EspMacAddress,
            Sensores = [.. payload.Sensores
                .Select(leitura => new LeituraTelemetriaDto
                {
                    NumeroEscova = leitura.NumeroEscova,
                    Temperatura = leitura.Temperatura
                })]
        };

        var telemetria = new TelemetriaRecebida(
            payloadSnapshot,
            ipAtual,
            dataRecebimento,
            requestId);

        if (!queue.TryEnqueue(telemetria))
        {
            logger.LogWarning(
                "Fila de telemetria cheia; requisição {RequestId} do ESP {MacAddress} não foi aceita.",
                requestId,
                payload.EspMacAddress);
            return StatusRecebimentoTelemetria.FilaCheia;
        }

        logger.LogInformation(
            "Telemetria do ESP {MacAddress} aceita para processamento. Requisição {RequestId}, {SensorCount} leituras.",
            payload.EspMacAddress,
            requestId,
            payload.Sensores.Count);

        return StatusRecebimentoTelemetria.Aceita;
    }
}
