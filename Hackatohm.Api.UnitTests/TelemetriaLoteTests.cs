using Hackatohm.Api.DTOs;
using Hackatohm.Api.DTOs.Hubs;
using Hackatohm.Api.Hubs;
using Hackatohm.Api.Queues;
using Hackatohm.Api.UseCases.Leituras;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Constants;
using Hackatohm.Domain.QualityGates.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hackatohm.Api.UnitTests;

public sealed class TelemetriaLoteTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProcessarAsync_DescartaLoteIncompletoOuComMotoresDiferentes(bool misturaMotores)
    {
        var sensores = new[] { CriarSensor(1, 12), CriarSensor(2, misturaMotores ? 13 : 12) };
        var store = new Store(sensores);
        var hub = new FakeHub();
        var evaluator = new FakeEvaluator();
        var useCase = CriarUseCase(store, hub, evaluator);
        var lote = CriarLote(misturaMotores ? [1, 2] : [1]);

        var resultado = await useCase.ProcessarAsync(lote);

        Assert.Equal(StatusProcessamentoLote.LoteInvalido, resultado.Status);
        Assert.Empty(store.Salvas);
        Assert.Equal(0, store.Commits);
        Assert.Equal(0, hub.Publicacoes);
        Assert.Empty(evaluator.Contextos);
        Assert.All(sensores, sensor => Assert.Null(sensor.UltimaConexao));
    }

    [Fact]
    public async Task ProcessarAsync_RegistraAlertasDoMotorComOsGatesDoLote()
    {
        var store = new Store([CriarSensor(1, 12), CriarSensor(2, 12)]);
        var alertas = new FakeAlertaService();
        var lote = CriarLote([1, 2]);

        await CriarUseCase(store, new FakeHub(), new FakeEvaluator(), alertas).ProcessarAsync(lote);

        var chamada = Assert.Single(alertas.Chamadas);
        Assert.Equal((12, 2, lote.DataRecebimento), chamada);
    }

    [Fact]
    public async Task ProcessarAsync_PublicaApenasOsAlertasNovosOuEscaladosOrdenadosPorSeveridade()
    {
        var store = new Store([CriarSensor(1, 12), CriarSensor(2, 12)]);
        var hub = new FakeHub();
        var alertas = new FakeAlertaService();
        alertas.Retorno.Add(CriarAlerta(10, NivelSeveridade.AlertaPreditivo, TiposRegraTemperatura.PrevisaoTendencia));
        alertas.Retorno.Add(CriarAlerta(11, NivelSeveridade.Critico, TiposRegraTemperatura.LimiarEstatico));

        await CriarUseCase(store, hub, new FakeEvaluator(), alertas).ProcessarAsync(CriarLote([1, 2]));

        Assert.Equal([11L, 10L], hub.Alertas.Select(alerta => alerta.Id));
        Assert.Equal(CategoriaAlerta.Condicao, hub.Alertas[0].Categoria);
        Assert.Equal(CategoriaAlerta.Preditivo, hub.Alertas[1].Categoria);
        Assert.True(hub.Alertas[0].Ativo);
    }

    [Fact]
    public async Task ProcessarAsync_NaoPublicaAlertaQuandoNadaAbriuNemEscalou()
    {
        var hub = new FakeHub();

        await CriarUseCase(new Store([CriarSensor(1, 12)]), hub, new FakeEvaluator())
            .ProcessarAsync(CriarLote([1]));

        Assert.Empty(hub.Alertas);
    }

    [Fact]
    public async Task ProcessarAsync_FalhaNoSignalRDoAlertaNaoDesfazAIngestao()
    {
        var store = new Store([CriarSensor(1, 12)]);
        var hub = new FakeHub { FalharAoPublicarAlerta = true };
        var alertas = new FakeAlertaService();
        alertas.Retorno.Add(CriarAlerta(10, NivelSeveridade.Critico, TiposRegraTemperatura.LimiarEstatico));

        var resultado = await CriarUseCase(store, hub, new FakeEvaluator(), alertas)
            .ProcessarAsync(CriarLote([1]));

        Assert.Equal(StatusProcessamentoLote.Sucesso, resultado.Status);
        Assert.Equal(1, store.Commits);
    }

    [Fact]
    public async Task ProcessarAsync_NaoRegistraAlertasQuandoOLoteEhRejeitado()
    {
        var store = new Store([CriarSensor(1, 12), CriarSensor(2, 12)]);
        var alertas = new FakeAlertaService();

        await CriarUseCase(store, new FakeHub(), new FakeEvaluator(), alertas)
            .ProcessarAsync(CriarLote([1]));

        Assert.Empty(alertas.Chamadas);
    }

    [Fact]
    public async Task ProcessarAsync_SalvaEPublicaTodosOsSensoresDoMotorComContextoCompleto()
    {
        var store = new Store([CriarSensor(1, 12), CriarSensor(2, 12)]);
        var hub = new FakeHub();
        var evaluator = new FakeEvaluator();
        var lote = CriarLote([1, 2]);

        var resultado = await CriarUseCase(store, hub, evaluator).ProcessarAsync(lote);

        Assert.Equal(StatusProcessamentoLote.Sucesso, resultado.Status);
        Assert.Equal(2, resultado.SensoresProcessados);
        Assert.Equal(2, store.Salvas.Count);
        Assert.Equal(1, store.Commits);
        Assert.Equal(1, hub.Publicacoes);
        Assert.All(store.Salvas, leitura => Assert.Equal(lote.DataRecebimento, leitura.DataHora));
        Assert.All(evaluator.Contextos, contexto => Assert.Equal(2, contexto.LeiturasIrmasDoLote!.Count));
    }

    private static AlertaManutencao CriarAlerta(long id, NivelSeveridade severidade, string tipoRegra) =>
        new()
        {
            Id = id,
            MotorId = 12,
            NomeRegra = "Regra de teste",
            TipoRegra = tipoRegra,
            CodigoRegra = "CODIGO",
            Severidade = severidade,
            Mensagem = "mensagem"
        };

    private static Sensor CriarSensor(int numero, int motor) =>
        new() { Id = Sensor.CriarId("AA:BB", numero), NumeroEscova = numero, MotorId = motor };

    private static TelemetriaRecebida CriarLote(int[] numeros) =>
        new(new ReceberTelemetriaRequestDto
        {
            EspMacAddress = "AA:BB",
            Sensores = numeros.Select(numero => new LeituraTelemetriaDto
            { NumeroEscova = numero, Temperatura = 40 + numero }).ToList()
        }, "127.0.0.1", DateTimeOffset.UtcNow, "teste-unitario");

    private static ProcessarLeiturasLoteUseCase CriarUseCase(
        Store store,
        FakeHub hub,
        FakeEvaluator evaluator,
        FakeAlertaService? alertas = null) =>
        new(store, store, store, hub, evaluator,
            new Hackatohm.Application.MotorStatus.MotorStatusService(null!, null!, evaluator),
            alertas ?? new FakeAlertaService(),
            Microsoft.Extensions.Options.Options.Create(new Hackatohm.Api.Queues.TelemetriaOptions()), NullLogger<ProcessarLeiturasLoteUseCase>.Instance);

    private sealed class FakeAlertaService : Hackatohm.Application.Alertas.IAlertaManutencaoService
    {
        public List<(int MotorId, int Sensores, DateTimeOffset Agora)> Chamadas { get; } = [];
        public List<AlertaManutencao> Retorno { get; } = [];

        public Task<IReadOnlyList<AlertaManutencao>> RegistrarAsync(
            int motorId,
            IReadOnlyList<Hackatohm.Application.Alertas.AvaliacaoSensor> avaliacoes,
            DateTimeOffset agora,
            CancellationToken cancellationToken = default)
        {
            Chamadas.Add((motorId, avaliacoes.Count, agora));
            return Task.FromResult<IReadOnlyList<AlertaManutencao>>([.. Retorno]);
        }
    }

    private sealed class Store(Sensor[] sensores) : ISensorRepository, ILeituraTemperaturaRepository, IUnitOfWork
    {
        public List<LeituraTemperatura> Salvas { get; } = [];
        public int Commits { get; private set; }
        public Task<IReadOnlyList<Sensor>> ObterPorMotorAsync(int motorId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Sensor>>(sensores.Where(sensor => sensor.MotorId == motorId).ToArray());
        public Task<IReadOnlyDictionary<string, Sensor>> ObterPorIdsComoDicionarioAsync(
            IReadOnlyCollection<string> ids, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, Sensor>>(sensores.Where(sensor => ids.Contains(sensor.Id))
                .ToDictionary(sensor => sensor.Id));
        public Task<Sensor?> ObterPorIdAsync(string id, CancellationToken cancellationToken = default) =>
            Task.FromResult(sensores.FirstOrDefault(sensor => sensor.Id == id));
        public Task<bool> ExisteAsync(string id, CancellationToken cancellationToken = default) =>
            Task.FromResult(sensores.Any(sensor => sensor.Id == id));
        public Task AdicionarAsync(Sensor sensor, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AdicionarRangeAsync(IEnumerable<LeituraTemperatura> leituras, CancellationToken cancellationToken = default)
        { Salvas.AddRange(leituras); return Task.CompletedTask; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(++Commits);
        public Task<IReadOnlyDictionary<string, IReadOnlyList<AmostraTemperatura>>> ObterHistoricoRecentePorSensoresAsync(
            IReadOnlyCollection<string> ids, DateTimeOffset? antesDe, int quantidade, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, IReadOnlyList<AmostraTemperatura>>>(
                new Dictionary<string, IReadOnlyList<AmostraTemperatura>>());
        public Task<IReadOnlyDictionary<string, LeituraTemperatura>> ObterMaisRecentesPorSensoresAsync(
            IReadOnlyCollection<string> ids, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, LeituraTemperatura>>(new Dictionary<string, LeituraTemperatura>());
        public Task<LeituraTemperatura?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<LeituraTemperatura?> ObterMaisRecentePorSensorAsync(string id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<LeituraTemperatura?> ObterAnteriorPorSensorAsync(string id, DateTimeOffset instante, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<LeituraTemperatura>> ObterPorMotorNoInstanteAsync(int id, DateTimeOffset instante, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeEvaluator : IQualityGateEvaluator
    {
        public IReadOnlyList<ContextoAvaliacao> Contextos { get; private set; } = [];
        public Task<IReadOnlyList<ResultadoGateQualidade>> AvaliarAsync(ContextoAvaliacao contexto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<IReadOnlyDictionary<string, IReadOnlyList<ResultadoGateQualidade>>> AvaliarLoteAsync(
            IReadOnlyList<ContextoAvaliacao> contextos, CancellationToken cancellationToken = default)
        {
            Contextos = contextos;
            return Task.FromResult<IReadOnlyDictionary<string, IReadOnlyList<ResultadoGateQualidade>>>(
                contextos.ToDictionary(contexto => contexto.Sensor.Id, _ => (IReadOnlyList<ResultadoGateQualidade>)Array.Empty<ResultadoGateQualidade>()));
        }
    }

    private sealed class FakeHub : IHubContext<TelemetriaHub, ITelemetriaClient>, IHubClients<ITelemetriaClient>, ITelemetriaClient
    {
        public int Publicacoes { get; private set; }
        public bool FalharAoPublicarAlerta { get; set; }
        public List<Hackatohm.Api.DTOs.AlertaManutencaoResponseDto> Alertas { get; } = [];
        IHubClients<ITelemetriaClient> IHubContext<TelemetriaHub, ITelemetriaClient>.Clients => this;
        IGroupManager IHubContext<TelemetriaHub, ITelemetriaClient>.Groups => throw new NotSupportedException();
        public ITelemetriaClient All => this;
        public Task ReceberLoteTelemetria(IEnumerable<TelemetriaRealtimeDto> leituras)
        { Publicacoes++; return Task.CompletedTask; }
        public Task ReceberTelemetria(TelemetriaRealtimeDto leitura) => throw new NotSupportedException();
        public Task ReceberStatusMotor(Hackatohm.Api.DTOs.MotorStatus.MotorStatusResponseDto status) => Task.CompletedTask;
        public Task ReceberAlertaManutencao(Hackatohm.Api.DTOs.AlertaManutencaoResponseDto alerta)
        {
            if (FalharAoPublicarAlerta)
                throw new InvalidOperationException("Falha simulada no SignalR.");
            Alertas.Add(alerta);
            return Task.CompletedTask;
        }
        public ITelemetriaClient AllExcept(IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
        public ITelemetriaClient Client(string connectionId) => throw new NotSupportedException();
        public ITelemetriaClient Clients(IReadOnlyList<string> connectionIds) => throw new NotSupportedException();
        public ITelemetriaClient Group(string groupName) => throw new NotSupportedException();
        public ITelemetriaClient GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
        public ITelemetriaClient Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();
        public ITelemetriaClient User(string userId) => throw new NotSupportedException();
        public ITelemetriaClient Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();
    }
}
