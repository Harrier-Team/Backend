using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.Queues;

public sealed record TelemetriaRecebida(
    ReceberTelemetriaRequestDto Payload,
    string? IpAtual,
    DateTime DataRecebimento,
    string RequestId);