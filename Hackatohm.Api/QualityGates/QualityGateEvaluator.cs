using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;

namespace Hackatohm.Domain.QualityGates;

/// <summary>
/// Localiza as regras ativas do motor e delega a avaliação às estratégias registradas.
/// </summary>
public sealed class QualityGateEvaluator(
    IRegraQualidadeRepository regraQualidadeRepository,
    IEnumerable<IQualityGateRule> regras,
    ILogger<QualityGateEvaluator> logger) : IQualityGateEvaluator
{
    private readonly IReadOnlyDictionary<string, IQualityGateRule> _regras =
        CriarIndiceRegras(regras);

    public async Task<IReadOnlyList<ResultadoGateQualidade>> AvaliarAsync(
        ContextoAvaliacao contexto,
        CancellationToken cancellationToken = default)
    {
        var resultadosLote = await AvaliarLoteAsync([contexto], cancellationToken);
        return resultadosLote[contexto.Sensor.Id];
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<ResultadoGateQualidade>>> AvaliarLoteAsync(
        IReadOnlyList<ContextoAvaliacao> contextos,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contextos);
        if (contextos.Count == 0)
            return new Dictionary<string, IReadOnlyList<ResultadoGateQualidade>>(
                StringComparer.OrdinalIgnoreCase);

        if (contextos.Any(contexto => contexto is null))
            throw new ArgumentException("O lote não pode conter contextos nulos.", nameof(contextos));

        var regrasPorMotor = await regraQualidadeRepository.ObterAtivasPorMotoresAsync(
            [.. contextos.Select(contexto => contexto.Sensor.MotorId).Distinct()],
            cancellationToken);

        var resultadosPorSensor = new Dictionary<string, IReadOnlyList<ResultadoGateQualidade>>(
            contextos.Count,
            StringComparer.OrdinalIgnoreCase);

        foreach (var contexto in contextos)
        {
            var resultados = new List<ResultadoGateQualidade>();
            if (regrasPorMotor.TryGetValue(contexto.Sensor.MotorId, out var regrasConfiguradas))
            {
                foreach (var regraConfigurada in regrasConfiguradas)
                {
                    if (!_regras.TryGetValue(regraConfigurada.TipoRegra, out var regra))
                    {
                        logger.LogError(
                            "A regra de qualidade {RegraId} usa o tipo não registrado {TipoRegra}.",
                            regraConfigurada.Id,
                            regraConfigurada.TipoRegra);
                        throw new InvalidOperationException(
                            $"Não há uma implementação registrada para a regra '{regraConfigurada.TipoRegra}' (ID {regraConfigurada.Id}).");
                    }

                    var resultado = await regra.AvaliarAsync(contexto, regraConfigurada.ParametrosJson);
                    resultados.Add(resultado.ComRegra(regraConfigurada.Id, regraConfigurada.Nome));
                }
            }

            if (!resultadosPorSensor.TryAdd(contexto.Sensor.Id, resultados))
                throw new ArgumentException(
                    $"O lote contém mais de um contexto para o sensor '{contexto.Sensor.Id}'.",
                    nameof(contextos));
        }

        return resultadosPorSensor;
    }

    private static IReadOnlyDictionary<string, IQualityGateRule> CriarIndiceRegras(
        IEnumerable<IQualityGateRule> regras)
    {
        ArgumentNullException.ThrowIfNull(regras);

        var indice = new Dictionary<string, IQualityGateRule>(StringComparer.Ordinal);
        foreach (var regra in regras)
        {
            if (!indice.TryAdd(regra.TipoRegra, regra))
            {
                throw new InvalidOperationException(
                    $"Há mais de uma implementação registrada para a regra '{regra.TipoRegra}'.");
            }
        }

        return indice;
    }
}