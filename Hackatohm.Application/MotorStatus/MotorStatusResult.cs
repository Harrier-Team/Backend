using Hackatohm.Domain.MotorStatus;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Application.MotorStatus;

public sealed record MotorStatusResult(
    int MotorId,
    NivelSeveridade SeveridadeGeral,
    SituacaoMotor Situacao,
    SeveridadeContagemResult Contagem,
    MotivoStatusResult? Motivo,
    IReadOnlyList<EscovaStatusResult> Escovas);

public sealed record SeveridadeContagemResult(
    int Normal,
    int Atencao,
    int AlertaPreditivo,
    int Critico);

public sealed record EscovaStatusResult(
    string SensorId,
    int NumeroEscova,
    double ValorCelsius,
    NivelSeveridade Severidade,
    DateTimeOffset DataHora,
    bool AlertaDesgaste,
    IReadOnlyList<ResultadoGateQualidade> ResultadosQualityGates);

public sealed record MotorStatusReading(
    string SensorId,
    int NumeroEscova,
    double ValorCelsius,
    DateTimeOffset DataHora,
    bool AlertaDesgaste,
    IReadOnlyList<ResultadoGateQualidade> ResultadosQualityGates);

public sealed record MotivoStatusResult(
    int NumeroEscova,
    string NomeRegra,
    string TipoRegra,
    string CodigoRegra,
    double? ValorObservado,
    double? ValorLimite,
    string MensagemDiagnostico);
