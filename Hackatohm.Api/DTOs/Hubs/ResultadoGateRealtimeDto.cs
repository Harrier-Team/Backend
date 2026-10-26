using Hackatohm.Api.QualityGates.Enums;

namespace Hackatohm.Api.DTOs.Hubs;

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
