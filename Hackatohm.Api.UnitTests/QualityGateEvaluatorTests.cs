using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Hackatohm.Application.QualityGates;
using Hackatohm.Domain.QualityGates.Constants;
using Hackatohm.Domain.QualityGates.Enums;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Rules;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hackatohm.Api.UnitTests;

public sealed class QualityGateEvaluatorTests
{
    [Fact]
    public async Task AvaliarLote_IgnoraRegraGlobalInativa()
    {
        var regra = new RegraQualidade
        {
            Id = Guid.NewGuid(),
            Nome = "Regra inativa",
            TipoRegra = TiposRegraTemperatura.LimiarEstatico,
            ParametrosJson = """{"aviso":80,"critico":95}""",
            Ativo = false
        };
        var repository = new FakeRegraRepository
        {
            RegrasPorMotor = new Dictionary<int, IReadOnlyList<RegraQualidade>>
            {
                [12] = []
            }
        };
        var evaluator = CriarEvaluator(repository);

        var results = await evaluator.AvaliarAsync(CriarContexto());

        Assert.Empty(results);
    }

    [Fact]
    public async Task AvaliarLote_MantemTodosOsResultadosAplicaveis()
    {
        var regra1 = CriarRegra("Regra 1", 80, 95);
        var regra2 = CriarRegra("Regra 2", 70, 90);
        var repository = new FakeRegraRepository
        {
            RegrasPorMotor = new Dictionary<int, IReadOnlyList<RegraQualidade>>
            {
                [12] = [regra1, regra2]
            }
        };
        var evaluator = CriarEvaluator(repository);

        var results = await evaluator.AvaliarAsync(CriarContexto(96));

        Assert.Equal(2, results.Count);
        Assert.All(results, result => Assert.Equal(NivelSeveridade.Critico, result.Severidade));
        Assert.Equal(regra1.Id, results[0].RegraQualidadeId);
        Assert.Equal(regra2.Id, results[1].RegraQualidadeId);
    }

    private static QualityGateEvaluator CriarEvaluator(FakeRegraRepository repository) =>
        new(repository, [new LimiarTemperaturaRule()], NullLogger<QualityGateEvaluator>.Instance);

    private static ContextoAvaliacao CriarContexto(double temperatura = 75) =>
        new(
            new Sensor { Id = "AA:BB_Escova1", NumeroEscova = 1, MotorId = 12 },
            new LeituraTelemetriaDto { NumeroEscova = 1, Temperatura = temperatura },
            DateTimeOffset.UtcNow);

    private static RegraQualidade CriarRegra(string nome, double aviso, double critico) =>
        new()
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            TipoRegra = TiposRegraTemperatura.LimiarEstatico,
            ParametrosJson = $$"""{"aviso":{{aviso}},"critico":{{critico}}}""",
            Ativo = true
        };

    private sealed class FakeRegraRepository : IRegraQualidadeRepository
    {
        public IReadOnlyDictionary<int, IReadOnlyList<RegraQualidade>> RegrasPorMotor { get; init; } =
            new Dictionary<int, IReadOnlyList<RegraQualidade>>();

        public Task<IReadOnlyDictionary<int, IReadOnlyList<RegraQualidade>>> ObterAtivasPorMotoresAsync(
            IReadOnlyCollection<int> motorIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(RegrasPorMotor);

        public Task<RegraQualidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<RegraQualidade?>(null);

        public Task<IReadOnlyList<RegraQualidade>> ListarAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<RegraQualidade>>([]);

        public Task AdicionarAsync(RegraQualidade regra, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Atualizar(RegraQualidade regra)
        {
        }

        public void Excluir(RegraQualidade regra)
        {
        }
    }
}
