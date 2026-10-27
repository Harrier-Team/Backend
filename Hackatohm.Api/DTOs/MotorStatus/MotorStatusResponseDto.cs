using Hackatohm.Domain.MotorStatus;
using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Api.DTOs.MotorStatus;

public sealed record MotorStatusResponseDto(
    int MotorId,
    NivelSeveridade SeveridadeGeral,
    SituacaoMotor Situacao,
    SeveridadeContagemResponseDto Contagem,
    MotivoStatusResponseDto? Motivo,
    IReadOnlyList<EscovaStatusResponseDto> Escovas);

public sealed record SeveridadeContagemResponseDto(
    int Normal,
    int Atencao,
    int AlertaPreditivo,
    int Critico);

public sealed record MotivoStatusResponseDto(
    int NumeroEscova,
    string NomeRegra,
    string TipoRegra,
    string CodigoRegra,
    double? ValorObservado,
    double? ValorLimite,
    string MensagemDiagnostico);

public sealed record EscovaStatusResponseDto(
    string SensorId,
    int NumeroEscova,
    double ValorCelsius,
    NivelSeveridade Severidade,
    DateTimeOffset DataHora,
    bool AlertaDesgaste,
    IReadOnlyList<Hubs.ResultadoGateRealtimeDto> ResultadosQualityGates);
