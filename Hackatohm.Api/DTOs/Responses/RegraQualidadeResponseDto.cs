using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Api.DTOs;

/// <summary>Representa uma regra de qualidade do catálogo.</summary>
public sealed record RegraQualidadeResponseDto(
    Guid Id,
    string Nome,
    string? Descricao,
    string TipoRegra,
    NivelSeveridade SeveridadePadrao,
    ParametrosLimiarTemperaturaDto Parametros,
    bool Ativo,
    DateTimeOffset CriadoEm);
