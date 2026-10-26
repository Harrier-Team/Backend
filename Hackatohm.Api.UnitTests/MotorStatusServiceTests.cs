using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Application.MotorStatus;
using Hackatohm.Application.QualityGates;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.MotorStatus;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Enums;
using Hackatohm.Domain.QualityGates.Rules;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hackatohm.Api.UnitTests;

public sealed class MotorStatusServiceTests
{
    [Fact]
    public async Task ObterAsync_AgregaSeveridadeContagemEMotivo()
    {
        var data = DateTimeOffset.UtcNow;
        var motor = CriarMotor();
        var leituras = CriarLeituras(motor, data, data);
        var evaluator = CriarEvaluator();
        var service = new MotorStatusService(
            new FakeMotorRepository(motor),
            new FakeLeituraRepository(leituras),
            evaluator);

        var resultado = await service.ObterAsync(
            motor.Id,
            TimeSpan.FromMinutes(2),
            data.AddSeconds(10));

        Assert.NotNull(resultado);
        Assert.Equal(NivelSeveridade.Critico, resultado.SeveridadeGeral);
        Assert.Equal(1, resultado.Contagem.Normal);
        Assert.Equal(1, resultado.Contagem.Critico);
        Assert.Equal(1, resultado.Motivo!.NumeroEscova);
        Assert.Equal("TEMP_CRITICA", resultado.Motivo.CodigoRegra);
        Assert.Equal(SituacaoMotor.Online, resultado.Situacao);
    }

    [Fact]
    public async Task ObterAsync_MarcaMotorComoDegradadoQuandoParteDasEscovasEstaAtrasada()
    {
        var data = DateTimeOffset.UtcNow;
        var motor = CriarMotor();
        var leituras = CriarLeituras(motor, data, data.AddMinutes(-3));
        var service = new MotorStatusService(
            new FakeMotorRepository(motor),
            new FakeLeituraRepository(leituras),
            CriarEvaluator());

        var resultado = await service.ObterAsync(
            motor.Id,
            TimeSpan.FromMinutes(2),
            data);

        Assert.Equal(SituacaoMotor.Degradado, resultado!.Situacao);
    }

    private static Motor CriarMotor() =>
        new()
        {
            Id = 3,
            Nome = "Motor de teste",
            Sensores =
            [
                new() { Id = "sensor-1", MotorId = 3, NumeroEscova = 1 },
                new() { Id = "sensor-2", MotorId = 3, NumeroEscova = 2 }
            ]
        };

    private static Dictionary<string, LeituraTemperatura> CriarLeituras(
        Motor motor,
        DateTimeOffset primeira,
        DateTimeOffset segunda) =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["sensor-1"] = new()
            {
                SensorId = motor.Sensores.ElementAt(0).Id,
                ValorCelsius = 96,
                DataHora = primeira,
                AlertaDesgaste = true
            },
            ["sensor-2"] = new()
            {
                SensorId = motor.Sensores.ElementAt(1).Id,
                ValorCelsius = 75,
                DataHora = segunda
            }
        };

    private static QualityGateEvaluator CriarEvaluator() =>
        new(
            new FakeRegraRepository(),
            [new LimiarTemperaturaRule()],
            NullLogger<QualityGateEvaluator>.Instance);

    private sealed class FakeMotorRepository(Motor motor) : IMotorRepository
    {
        public Task<Motor?> ObterComSensoresAsync(int motorId, CancellationToken cancellationToken = default) =>
            Task.FromResult(motorId == motor.Id ? motor : null);

        public Task AdicionarAsync(Motor motor, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExisteAsync(int motorId, CancellationToken cancellationToken = default) => Task.FromResult(motorId == motor.Id);
        public Task<IReadOnlyList<string>> ObterIdsSensoresExistentesAsync(IReadOnlyCollection<string> sensorIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<string>>([]);
        public Task<IReadOnlyList<Motor>> ListarComSensoresAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Motor>>([motor]);
        public Task<Motor?> ObterParaRemocaoAsync(int motorId, CancellationToken cancellationToken = default) => Task.FromResult<Motor?>(motor);
        public void Remover(Motor motor)
        {
        }
    }

    private sealed class FakeLeituraRepository(
        IReadOnlyDictionary<string, LeituraTemperatura> leituras) : ILeituraTemperaturaRepository
    {
        public Task<IReadOnlyDictionary<string, LeituraTemperatura>> ObterMaisRecentesPorSensoresAsync(
            IReadOnlyCollection<string> sensorIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(leituras);

        public Task AdicionarRangeAsync(IEnumerable<LeituraTemperatura> leituras, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<LeituraTemperatura?> ObterPorIdAsync(long leituraId, CancellationToken cancellationToken = default) => Task.FromResult<LeituraTemperatura?>(null);
        public Task<LeituraTemperatura?> ObterMaisRecentePorSensorAsync(string sensorId, CancellationToken cancellationToken = default) => Task.FromResult<LeituraTemperatura?>(null);
    }

    private sealed class FakeRegraRepository : IRegraQualidadeRepository
    {
        public Task<IReadOnlyDictionary<int, IReadOnlyList<RegraQualidade>>> ObterAtivasPorMotoresAsync(
            IReadOnlyCollection<int> motorIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<RegraQualidade>>>(
                new Dictionary<int, IReadOnlyList<RegraQualidade>>
                {
                    [3] =
                    [
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Nome = "Limiar de teste",
                            TipoRegra = "temperatura.limiar_estatico",
                            ParametrosJson = """{"aviso":80,"critico":95}""",
                            Ativo = true
                        }
                    ]
                });

        public Task<RegraQualidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<RegraQualidade?>(null);
        public Task<IReadOnlyList<RegraQualidade>> ListarAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<RegraQualidade>>([]);
        public Task AdicionarAsync(RegraQualidade regra, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Atualizar(RegraQualidade regra)
        {
        }
        public void Excluir(RegraQualidade regra)
        {
        }
    }
}
