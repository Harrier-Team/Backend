using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Api.DTOs.Hubs;

/// <summary>Representa o resultado transitório de uma regra publicado via SignalR.</summary>
public sealed record ResultadoGateRealtimeDto(
    Guid RegraQualidadeId,
    string NomeRegra,
    bool DisparouAlerta,
    NivelSeveridade Severidade,
    string TipoRegra,
    string CodigoRegra,
    string MensagemDiagnostico,
    double? ValorObservado,
    double? ValorLimite,
    IReadOnlyDictionary<string, object>? Metadados);
