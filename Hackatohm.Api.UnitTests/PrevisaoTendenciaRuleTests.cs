using Hackatohm.Api.DTOs;
using Hackatohm.Api.Validation;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Constants;
using Hackatohm.Domain.QualityGates.Enums;
using Hackatohm.Domain.QualityGates.Rules;

namespace Hackatohm.Api.UnitTests;

public sealed class PrevisaoTendenciaRuleTests
{
    private const string Parametros =
        """{"aviso":30,"critico":10,"temperaturaLimite":95,"janela":12}""";

    private static readonly DateTimeOffset Agora = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task InformaDadosInsuficientesComPoucasLeituras()
    {
        var resultado = await new PrevisaoTendenciaRule().AvaliarAsync(
            CriarContexto(temperatura: 50, historico: Serie(inicio: 48, passoCelsius: 0.5, quantidade: 2)),
            Parametros);

        Assert.False(resultado.DisparouAlerta);
        Assert.Equal("DADOS_INSUFICIENTES", resultado.CodigoRegra);
        Assert.Equal(false, resultado.Metadados!["avaliacaoConcluida"]);
    }

    [Fact]
    public async Task EstimaMinutosAteOLimiteComRetaCrescente()
    {
        // +1 °C a cada 30 s = 2 °C/min; atual 75 °C => faltam 20 °C => 10 min.
        var historico = Serie(inicio: 65, passoCelsius: 1, quantidade: 10);
        var resultado = await new PrevisaoTendenciaRule().AvaliarAsync(
            CriarContexto(temperatura: 75, historico: historico),
            Parametros);

        Assert.Equal(NivelSeveridade.Critico, resultado.Severidade);
        Assert.Equal("PREVISAO_TENDENCIA_CRITICA", resultado.CodigoRegra);
        Assert.Equal(10, resultado.ValorObservado!.Value, precision: 6);
        Assert.Equal(10, resultado.ValorLimite);
        Assert.Equal(2, (double)resultado.Metadados!["inclinacaoCelsiusPorMinuto"], precision: 6);
    }

    [Fact]
    public async Task ClassificaComoAlertaPreditivoQuandoEstaNoHorizonteDeAviso()
    {
        // 2 °C/min, atual 55 °C => 20 min até 95 °C (entre 10 e 30).
        var historico = Serie(inicio: 45, passoCelsius: 1, quantidade: 10);
        var resultado = await new PrevisaoTendenciaRule().AvaliarAsync(
            CriarContexto(temperatura: 55, historico: historico),
            Parametros);

        Assert.Equal(NivelSeveridade.AlertaPreditivo, resultado.Severidade);
        Assert.Equal("PREVISAO_TENDENCIA_ALERTA", resultado.CodigoRegra);
        Assert.Equal(20, resultado.ValorObservado!.Value, precision: 6);
        Assert.Equal(30, resultado.ValorLimite);
    }

    [Fact]
    public async Task FicaNormalQuandoOLimiteEstaMuitoLonge()
    {
        // 2 °C/min, atual 40 °C => 27,5 min até 95 °C, acima do horizonte de aviso (10 min).
        var historico = Serie(inicio: 30, passoCelsius: 1, quantidade: 10);
        var resultado = await new PrevisaoTendenciaRule().AvaliarAsync(
            CriarContexto(temperatura: 40, historico: historico),
            """{"aviso":10,"critico":5,"temperaturaLimite":95}""");

        Assert.Equal(NivelSeveridade.Normal, resultado.Severidade);
        Assert.False(resultado.DisparouAlerta);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task FicaNormalQuandoATemperaturaEstaEstavelOuCaindo(double passoCelsius)
    {
        var historico = Serie(inicio: 90, passoCelsius: passoCelsius, quantidade: 10);
        var resultado = await new PrevisaoTendenciaRule().AvaliarAsync(
            CriarContexto(temperatura: 90 + passoCelsius * 10, historico: historico),
            Parametros);

        Assert.Equal(NivelSeveridade.Normal, resultado.Severidade);
    }

    [Fact]
    public async Task LimiteJaUltrapassadoComTendenciaDeAltaEhCritico()
    {
        var historico = Serie(inicio: 92, passoCelsius: 0.5, quantidade: 10);
        var resultado = await new PrevisaoTendenciaRule().AvaliarAsync(
            CriarContexto(temperatura: 97, historico: historico),
            Parametros);

        Assert.Equal(NivelSeveridade.Critico, resultado.Severidade);
        Assert.Equal(0, resultado.ValorObservado);
    }

    [Fact]
    public async Task UsaApenasAsLeiturasDaJanela()
    {
        // Leituras antigas em queda não podem esconder a subida recente.
        var antigas = Serie(inicio: 100, passoCelsius: -1, quantidade: 30, inicioOffset: 60);
        var recentes = Serie(inicio: 60, passoCelsius: 1, quantidade: 10);
        var resultado = await new PrevisaoTendenciaRule().AvaliarAsync(
            CriarContexto(temperatura: 70, historico: [.. antigas, .. recentes]),
            """{"aviso":30,"critico":10,"temperaturaLimite":95,"janela":8}""");

        Assert.Equal(NivelSeveridade.AlertaPreditivo, resultado.Severidade);
        Assert.Equal(12.5, resultado.ValorObservado!.Value, precision: 6);
    }

    [Theory]
    [InlineData("""{"aviso":10,"critico":30,"temperaturaLimite":95}""")]
    [InlineData("""{"aviso":30,"critico":0,"temperaturaLimite":95}""")]
    [InlineData("""{"aviso":30,"critico":10,"temperaturaLimite":500}""")]
    [InlineData("""{"aviso":30,"critico":10,"temperaturaLimite":95,"janela":2}""")]
    public async Task RejeitaConfiguracaoInvalida(string parametros)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await new PrevisaoTendenciaRule().AvaliarAsync(
                CriarContexto(temperatura: 50, historico: Serie(40, 1, 10)),
                parametros));
    }

    [Fact]
    public void ValidadorAceitaParametrosDeTendencia()
    {
        var resultado = new CadastrarRegraQualidadeRequestDtoValidator().Validate(
            CriarRequest(new ParametrosLimiarTemperaturaDto(30, 10, 95, 12)));

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData(10, 30, 95.0)]
    [InlineData(30, 10, null)]
    [InlineData(30, 0, 95.0)]
    [InlineData(30, 10, 250.0)]
    public void ValidadorRejeitaParametrosDeTendenciaInvalidos(double aviso, double critico, double? limite)
    {
        var resultado = new CadastrarRegraQualidadeRequestDtoValidator().Validate(
            CriarRequest(new ParametrosLimiarTemperaturaDto(aviso, critico, limite)));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void ValidadorRejeitaCamposDeTendenciaEmOutroTipoDeRegra()
    {
        var request = CriarRequest(new ParametrosLimiarTemperaturaDto(80, 95, 100)) with
        {
            TipoRegra = TiposRegraTemperatura.LimiarEstatico
        };

        Assert.False(new CadastrarRegraQualidadeRequestDtoValidator().Validate(request).IsValid);
    }

    private static CadastrarRegraQualidadeRequestDto CriarRequest(ParametrosLimiarTemperaturaDto parametros) =>
        new()
        {
            Nome = "Previsão por tendência",
            TipoRegra = TiposRegraTemperatura.PrevisaoTendencia,
            Parametros = parametros
        };

    /// <summary>Leituras a cada 30 s, terminando 30 s antes de <see cref="Agora"/>.</summary>
    private static AmostraTemperatura[] Serie(
        double inicio,
        double passoCelsius,
        int quantidade,
        int inicioOffset = 0) =>
        Enumerable.Range(0, quantidade)
            .Select(indice => new AmostraTemperatura(
                Agora.AddSeconds(-30 * (quantidade - indice + inicioOffset)),
                inicio + passoCelsius * indice))
            .ToArray();

    private static ContextoAvaliacao CriarContexto(
        double temperatura,
        IReadOnlyList<AmostraTemperatura> historico) =>
        new(
            new Sensor { Id = "AA:BB_Escova1", NumeroEscova = 1, MotorId = 12 },
            new LeituraTelemetriaDto { NumeroEscova = 1, Temperatura = temperatura },
            Agora,
            HistoricoRecente: historico);
}
