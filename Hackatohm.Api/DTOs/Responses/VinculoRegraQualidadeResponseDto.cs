namespace Hackatohm.Api.DTOs;

/// <summary>Representa a configuração de uma regra associada a um motor.</summary>
public sealed record VinculoRegraQualidadeResponseDto(
    int MotorId,
    Guid RegraQualidadeId,
    bool Ativo,
    DateTimeOffset VinculadoEm,
    RegraQualidadeResponseDto Regra);
