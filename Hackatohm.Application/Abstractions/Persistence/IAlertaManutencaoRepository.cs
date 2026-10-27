using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Application.Abstractions.Persistence;

/// <param name="MotorId">Restringe a um motor.</param>
/// <param name="Ativo">Verdadeiro para alertas em aberto, falso para encerrados.</param>
/// <param name="TiposRegra">Restringe aos tipos de regra informados; lista vazia não retorna nada.</param>
/// <param name="Severidade">Restringe à severidade atual do alerta.</param>
public sealed record FiltroAlertas(
    int? MotorId = null,
    bool? Ativo = null,
    IReadOnlyCollection<string>? TiposRegra = null,
    NivelSeveridade? Severidade = null);

public sealed record PaginaAlertas(IReadOnlyList<AlertaManutencao> Itens, int Total);

public interface IAlertaManutencaoRepository
{
    /// <summary>Alertas em aberto do motor, rastreados para atualização.</summary>
    Task<IReadOnlyList<AlertaManutencao>> ObterAbertosPorMotorAsync(
        int motorId,
        CancellationToken cancellationToken = default);

    Task AdicionarRangeAsync(
        IEnumerable<AlertaManutencao> alertas,
        CancellationToken cancellationToken = default);

    /// <summary>Lista do mais recente para o mais antigo, com paginação a partir de 1.</summary>
    Task<PaginaAlertas> ListarAsync(
        FiltroAlertas filtro,
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default);
}
