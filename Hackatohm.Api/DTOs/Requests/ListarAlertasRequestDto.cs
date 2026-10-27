using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Api.DTOs;

/// <summary>Filtros e paginação da listagem de alertas de manutenção.</summary>
public sealed class ListarAlertasRequestDto
{
    public const int TamanhoPadrao = 50;
    public const int TamanhoMaximo = 200;

    public int? MotorId { get; set; }

    /// <summary>Verdadeiro para alertas em aberto, falso para encerrados; omitido retorna ambos.</summary>
    public bool? Ativo { get; set; }
    public CategoriaAlerta? Categoria { get; set; }
    public string? TipoRegra { get; set; }
    public NivelSeveridade? Severidade { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamanho { get; set; } = TamanhoPadrao;
}
