using FluentValidation;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.UseCases.Leituras;
using Hackatohm.Api.Validation;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Application.QualityGates;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates.Rules;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hackatohm.Api.UnitTests;

public sealed class ObterUltimaLeituraUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_RetornaQualityGateDaUltimaLeitura()
    {
        var sensor = new Sensor
        {
            Id = "AA:BB_Escova1",
            NumeroEscova = 1,
            MotorId = 12
        };
        var leitura = new LeituraTemperatura
        {
            SensorId = sensor.Id,
            Sensor = sensor,
            ValorCelsius = 56.75,
            DataHora = DateTimeOffset.UtcNow,
            AlertaDesgaste = true
        };
        var regra = new RegraQualidade
        {
            Id = Guid.NewGuid(),
            Nome = "Limiar térmico",
            TipoRegra = "temperatura.limiar_estatico",
            ParametrosJson = """{"aviso":40,"critico":60}""",
            Ativo = true
        };
        var evaluator = new QualityGateEvaluator(
            new FakeRegraRepository(regra),
            [new LimiarTemperaturaRule()],
            NullLogger<QualityGateEvaluator>.Instance);
        var useCase = new ObterUltimaLeituraUseCase(
            new FakeLeituraRepository(leitura),
            new FakeSensorRepository(sensor),
            evaluator,
            new ObterUltimaLeituraRequestDtoValidator());

        var result = await useCase.ExecutarAsync(new ObterUltimaLeituraRequestDto
        {
            EspMacAddress = "AA:BB",
            NumeroEscova = 1
        });

        Assert.NotNull(result);
        Assert.Equal(12, result.MotorId);
        var gate = Assert.Single(result.ResultadosQualityGates);
        Assert.Equal("AlertaPreditivo", gate.Severidade.ToString());
        Assert.True(gate.DisparouAlerta);
        Assert.Equal(56.75, gate.ValorObservado);
    }

    [Theory]
    [InlineData("temperatura.gradiente_termico", 20)]
    [InlineData("temperatura.assimetria_escovas", 35)]
    public async Task ExecutarAsync_ReconstroiContextoEReavaliaAlertaNaRecarga(string tipo, double observado)
    {
        var agora = DateTimeOffset.UtcNow;
        var sensor = new Sensor { Id = "AA:BB_Escova1", NumeroEscova = 1, MotorId = 12 };
        var irmao = new Sensor { Id = "AA:BB_Escova2", NumeroEscova = 2, MotorId = 12 };
        var leitura = new LeituraTemperatura
        {
            Id = 3, SensorId = sensor.Id, Sensor = sensor,
            DataHora = agora, ValorCelsius = 80, AlertaDesgaste = false
        };
        var anterior = new LeituraTemperatura
        {
            Id = 1, SensorId = sensor.Id, Sensor = sensor,
            DataHora = agora.AddMinutes(-1), ValorCelsius = 60
        };
        var outra = new LeituraTemperatura
        {
            Id = 4, SensorId = irmao.Id, Sensor = irmao,
            DataHora = agora, ValorCelsius = 45
        };
        var regra = new RegraQualidade
        {
            Id = Guid.NewGuid(), Nome = "Regra de tendência", TipoRegra = tipo,
            ParametrosJson = """{"aviso":10,"critico":15}""", Ativo = true
        };
        var evaluator = new QualityGateEvaluator(
            new FakeRegraRepository(regra),
            [new GradienteTemperaturaRule(), new AssimetriaEscovasRule()],
            NullLogger<QualityGateEvaluator>.Instance);
        var useCase = new ObterUltimaLeituraUseCase(
            new FakeLeituraRepository(leitura, anterior, outra),
            new FakeSensorRepository(sensor), evaluator,
            new ObterUltimaLeituraRequestDtoValidator());
        var request = new ObterUltimaLeituraRequestDto { EspMacAddress = "AA:BB", NumeroEscova = 1 };

        for (var recarga = 0; recarga < 2; recarga++)
        {
            var result = await useCase.ExecutarAsync(request);
            Assert.NotNull(result);
            Assert.True(result.AlertaDesgaste);
            var gate = Assert.Single(result.ResultadosQualityGates);
            Assert.Equal("Critico", gate.Severidade.ToString());
            Assert.Equal(observado, gate.ValorObservado);
        }

        regra.ParametrosJson = """{"aviso":90,"critico":100}""";
        var reavaliado = await useCase.ExecutarAsync(request);
        Assert.NotNull(reavaliado);
        Assert.False(reavaliado.AlertaDesgaste);
        Assert.Equal("Normal", Assert.Single(reavaliado.ResultadosQualityGates).Severidade.ToString());
    }

    private sealed class FakeSensorRepository(Sensor sensor) : ISensorRepository
    {
        public Task<IReadOnlyList<Sensor>> ObterPorMotorAsync(
            int motorId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Sensor>>(motorId == sensor.MotorId ? [sensor] : []);

        public Task<Sensor?> ObterPorIdAsync(string sensorId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Sensor?>(sensor.Id == sensorId ? sensor : null);

        public Task AdicionarAsync(Sensor sensor, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> ExisteAsync(string sensorId, CancellationToken cancellationToken = default) =>
            Task.FromResult(sensor.Id == sensorId);

        public Task<IReadOnlyDictionary<string, Sensor>> ObterPorIdsComoDicionarioAsync(
            IReadOnlyCollection<string> sensorIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, Sensor>>(
                sensorIds.Contains(sensor.Id)
                    ? new Dictionary<string, Sensor> { [sensor.Id] = sensor }
                    : new Dictionary<string, Sensor>());
    }

    private sealed class FakeLeituraRepository(params LeituraTemperatura[] leituras) : ILeituraTemperaturaRepository
    {
        public Task<LeituraTemperatura?> ObterMaisRecentePorSensorAsync(
            string sensorId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(leituras.Where(leitura => leitura.SensorId == sensorId)
                .OrderByDescending(leitura => leitura.DataHora).FirstOrDefault());

        public Task<LeituraTemperatura?> ObterAnteriorPorSensorAsync(
            string sensorId, DateTimeOffset instante, CancellationToken cancellationToken = default) =>
            Task.FromResult(leituras.Where(leitura => leitura.SensorId == sensorId && leitura.DataHora < instante)
                .OrderByDescending(leitura => leitura.DataHora).FirstOrDefault());

        public Task<IReadOnlyList<LeituraTemperatura>> ObterPorMotorNoInstanteAsync(
            int motorId, DateTimeOffset instante, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<LeituraTemperatura>>(leituras
                .Where(leitura => leitura.Sensor.MotorId == motorId && leitura.DataHora == instante).ToArray());

        public Task AdicionarRangeAsync(
            IEnumerable<LeituraTemperatura> leituras,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<LeituraTemperatura?> ObterPorIdAsync(
            long leituraId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<LeituraTemperatura?>(null);

        public Task<IReadOnlyDictionary<string, LeituraTemperatura>> ObterMaisRecentesPorSensoresAsync(
            IReadOnlyCollection<string> sensorIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, LeituraTemperatura>>(
                leituras.Where(leitura => sensorIds.Contains(leitura.SensorId))
                    .GroupBy(leitura => leitura.SensorId)
                    .ToDictionary(grupo => grupo.Key, grupo => grupo.OrderByDescending(leitura => leitura.DataHora).First()));
    }

    private sealed class FakeRegraRepository(RegraQualidade regra) : IRegraQualidadeRepository
    {
        public Task<IReadOnlyDictionary<int, IReadOnlyList<RegraQualidade>>> ObterAtivasPorMotoresAsync(
            IReadOnlyCollection<int> motorIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<RegraQualidade>>>(
                motorIds.Contains(12)
                    ? new Dictionary<int, IReadOnlyList<RegraQualidade>> { [12] = [regra] }
                    : new Dictionary<int, IReadOnlyList<RegraQualidade>>());

        public Task<RegraQualidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<RegraQualidade?>(id == regra.Id ? regra : null);

        public Task<IReadOnlyList<RegraQualidade>> ListarAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<RegraQualidade>>([regra]);

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
