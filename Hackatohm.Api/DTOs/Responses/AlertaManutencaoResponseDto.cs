using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Api.DTOs;

/// <summary>Alerta de manutenção: um gate que passou a disparar e, se já terminou, quando.</summary>
public sealed record AlertaManutencaoResponseDto(
    long Id,
    int MotorId,
    string? SensorId,
    int? NumeroEscova,
    Guid? RegraQualidadeId,
    string NomeRegra,
    string TipoRegra,
    CategoriaAlerta Categoria,
    string CodigoRegra,
    NivelSeveridade Severidade,
    double? ValorObservado,
    double? ValorLimite,
    string Mensagem,
    DateTimeOffset IniciadoEm,
    DateTimeOffset AtualizadoEm,
    DateTimeOffset? ResolvidoEm,
    bool Ativo);

public sealed record ListaPaginadaResponseDto<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int Tamanho,
    int Total);
