using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Application.Alertas;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Constants;
using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Api.UnitTests;

public sealed class AlertaManutencaoServiceTests
{
    private static readonly Guid RegraLimiar = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid RegraAssimetria = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTimeOffset T0 = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeAlertaRepository repositorio = new();
    private readonly AlertaManutencaoService servico;

    public AlertaManutencaoServiceTests() => servico = new(repositorio);

    [Fact]
    public async Task AbreAlertaQuandoOGateDispara()
    {
        var sensor = CriarSensor(2);

        var novos = await servico.RegistrarAsync(
            12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 96))], T0);

        var alerta = Assert.Single(novos);
        Assert.Same(alerta, Assert.Single(repositorio.Todos));
        Assert.Equal(12, alerta.MotorId);
        Assert.Equal(sensor.Id, alerta.SensorId);
        Assert.Equal(2, alerta.NumeroEscova);
        Assert.Equal(RegraLimiar, alerta.RegraQualidadeId);
        Assert.Equal("Limiar de bancada", alerta.NomeRegra);
        Assert.Equal(NivelSeveridade.Critico, alerta.Severidade);
        Assert.Equal(96, alerta.ValorObservado);
        Assert.Equal(T0, alerta.IniciadoEm);
        Assert.True(alerta.Ativo);
    }

    [Fact]
    public async Task NaoAbreAlertaParaGateQueNaoDispara()
    {
        var novos = await servico.RegistrarAsync(
            12, [Avaliar(CriarSensor(1), ResultadoGateQualidade.Normal(TiposRegraTemperatura.LimiarEstatico)
                .ComRegra(RegraLimiar, "Limiar de bancada"))], T0);

        Assert.Empty(novos);
        Assert.Empty(repositorio.Todos);
    }

    [Fact]
    public async Task MantemOMesmoAlertaEAtualizaValoresSemNotificar()
    {
        var sensor = CriarSensor(1);
        await servico.RegistrarAsync(12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 96))], T0);

        var novos = await servico.RegistrarAsync(
            12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 98))], T0.AddSeconds(10));

        Assert.Empty(novos);
        var alerta = Assert.Single(repositorio.Todos);
        Assert.Equal(98, alerta.ValorObservado);
        Assert.Equal(T0, alerta.IniciadoEm);
        Assert.Equal(T0.AddSeconds(10), alerta.AtualizadoEm);
        Assert.True(alerta.Ativo);
    }

    [Fact]
    public async Task NotificaQuandoASeveridadeSobe()
    {
        var sensor = CriarSensor(1);
        await servico.RegistrarAsync(12, [Avaliar(sensor, Limiar(NivelSeveridade.AlertaPreditivo, 85))], T0);

        var escalados = await servico.RegistrarAsync(
            12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 96))], T0.AddSeconds(10));

        var alerta = Assert.Single(escalados);
        Assert.Same(Assert.Single(repositorio.Todos), alerta);
        Assert.Equal(NivelSeveridade.Critico, alerta.Severidade);
    }

    [Fact]
    public async Task NaoNotificaQuandoASeveridadeDesce()
    {
        var sensor = CriarSensor(1);
        await servico.RegistrarAsync(12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 96))], T0);

        var novos = await servico.RegistrarAsync(
            12, [Avaliar(sensor, Limiar(NivelSeveridade.AlertaPreditivo, 85))], T0.AddSeconds(10));

        Assert.Empty(novos);
        Assert.Equal(NivelSeveridade.AlertaPreditivo, Assert.Single(repositorio.Todos).Severidade);
    }

    [Fact]
    public async Task EncerraOAlertaQuandoOGateDeixaDeDisparar()
    {
        var sensor = CriarSensor(1);
        await servico.RegistrarAsync(12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 96))], T0);

        var novos = await servico.RegistrarAsync(
            12,
            [Avaliar(sensor, ResultadoGateQualidade.Normal(TiposRegraTemperatura.LimiarEstatico)
                .ComRegra(RegraLimiar, "Limiar de bancada"))],
            T0.AddSeconds(10));

        Assert.Empty(novos);
        var alerta = Assert.Single(repositorio.Todos);
        Assert.Equal(T0.AddSeconds(10), alerta.ResolvidoEm);
        Assert.False(alerta.Ativo);
    }

    [Fact]
    public async Task ReabreUmNovoAlertaDepoisDeEncerrado()
    {
        var sensor = CriarSensor(1);
        await servico.RegistrarAsync(12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 96))], T0);
        await servico.RegistrarAsync(12, [Avaliar(sensor)], T0.AddSeconds(10));

        var novos = await servico.RegistrarAsync(
            12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 97))], T0.AddSeconds(20));

        Assert.Single(novos);
        Assert.Equal(2, repositorio.Todos.Count);
        Assert.Single(repositorio.Todos, alerta => alerta.Ativo);
    }

    [Fact]
    public async Task EncerraAlertaQuandoARegraDeixaDeSerAvaliada()
    {
        var sensor = CriarSensor(1);
        await servico.RegistrarAsync(12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 96))], T0);

        await servico.RegistrarAsync(12, [Avaliar(sensor)], T0.AddSeconds(10));

        Assert.False(Assert.Single(repositorio.Todos).Ativo);
    }

    [Fact]
    public async Task SensoresDiferentesGeramAlertasIndependentes()
    {
        var escova1 = CriarSensor(1);
        var escova2 = CriarSensor(2);

        var novos = await servico.RegistrarAsync(
            12,
            [
                Avaliar(escova1, Limiar(NivelSeveridade.Critico, 96)),
                Avaliar(escova2, Limiar(NivelSeveridade.AlertaPreditivo, 85))
            ],
            T0);

        Assert.Equal(2, novos.Count);

        await servico.RegistrarAsync(
            12, [Avaliar(escova1, Limiar(NivelSeveridade.Critico, 96)), Avaliar(escova2)], T0.AddSeconds(10));

        Assert.True(repositorio.Todos.Single(alerta => alerta.NumeroEscova == 1).Ativo);
        Assert.False(repositorio.Todos.Single(alerta => alerta.NumeroEscova == 2).Ativo);
    }

    [Fact]
    public async Task AssimetriaGeraUmUnicoAlertaPorMotorSemSensor()
    {
        var resultado = ResultadoGateQualidade.Critico(
                TiposRegraTemperatura.AssimetriaEscovas,
                "ASSIMETRIA_ESCOVAS_CRITICA",
                "A diferença entre as escovas do motor atingiu 25,0 °C.",
                25,
                20)
            .ComRegra(RegraAssimetria, "Assimetria");

        var novos = await servico.RegistrarAsync(
            12,
            [
                Avaliar(CriarSensor(1), resultado),
                Avaliar(CriarSensor(2), resultado),
                Avaliar(CriarSensor(3), resultado)
            ],
            T0);

        var alerta = Assert.Single(novos);
        Assert.Null(alerta.SensorId);
        Assert.Null(alerta.NumeroEscova);
        Assert.Equal(TiposRegraTemperatura.AssimetriaEscovas, alerta.TipoRegra);

        var seguinte = await servico.RegistrarAsync(
            12,
            [Avaliar(CriarSensor(1), resultado), Avaliar(CriarSensor(2), resultado)],
            T0.AddSeconds(10));

        Assert.Empty(seguinte);
        Assert.Single(repositorio.Todos);
    }

    [Fact]
    public async Task IgnoraAlertasAbertosDeOutroMotor()
    {
        var sensor = CriarSensor(1);
        await servico.RegistrarAsync(12, [Avaliar(sensor, Limiar(NivelSeveridade.Critico, 96))], T0);

        await servico.RegistrarAsync(13, [Avaliar(CriarSensor(1))], T0.AddSeconds(10));

        Assert.True(Assert.Single(repositorio.Todos).Ativo);
    }

    [Fact]
    public async Task TruncaMensagensMuitoLongas()
    {
        var resultado = ResultadoGateQualidade.Critico(
                TiposRegraTemperatura.LimiarEstatico, "TEMP_CRITICA", new string('x', 800), 96, 95)
            .ComRegra(RegraLimiar, "Limiar de bancada");

        await servico.RegistrarAsync(12, [Avaliar(CriarSensor(1), resultado)], T0);

        Assert.Equal(500, Assert.Single(repositorio.Todos).Mensagem.Length);
    }

    private static Sensor CriarSensor(int numeroEscova) =>
        new() { Id = Sensor.CriarId("AA:BB", numeroEscova), NumeroEscova = numeroEscova, MotorId = 12 };

    private static AvaliacaoSensor Avaliar(Sensor sensor, params ResultadoGateQualidade[] resultados) =>
        new(sensor, resultados);

    private static ResultadoGateQualidade Limiar(NivelSeveridade severidade, double temperatura) =>
        (severidade == NivelSeveridade.Critico
            ? ResultadoGateQualidade.Critico(
                TiposRegraTemperatura.LimiarEstatico, "TEMP_CRITICA", $"Crítico {temperatura}", temperatura, 95)
            : ResultadoGateQualidade.AlertaPreditivo(
                TiposRegraTemperatura.LimiarEstatico, "TEMP_ALERTA_PREDITIVO", $"Alerta {temperatura}", temperatura, 80))
        .ComRegra(RegraLimiar, "Limiar de bancada");

    private sealed class FakeAlertaRepository : IAlertaManutencaoRepository
    {
        public List<AlertaManutencao> Todos { get; } = [];

        public Task<IReadOnlyList<AlertaManutencao>> ObterAbertosPorMotorAsync(
            int motorId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AlertaManutencao>>(
                [.. Todos.Where(alerta => alerta.MotorId == motorId && alerta.Ativo)]);

        public Task AdicionarRangeAsync(
            IEnumerable<AlertaManutencao> alertas, CancellationToken cancellationToken = default)
        {
            Todos.AddRange(alertas);
            return Task.CompletedTask;
        }

        public Task<PaginaAlertas> ListarAsync(
            FiltroAlertas filtro, int pagina, int tamanho, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
