using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Enums;
using Hackatohm.Domain.QualityGates.Rules;

namespace Hackatohm.Api.UnitTests;

public sealed class QualityGateRulesTests
{
    [Fact]
    public async Task GradienteTemperaturaRule_DisparaAlertaComTaxaDeSubida()
    {
        var contexto = CriarContexto(
            temperatura: 50,
            temperaturaAnterior: 40,
            intervalo: TimeSpan.FromMinutes(2));

        var resultado = await new GradienteTemperaturaRule().AvaliarAsync(
            contexto,
            """{"aviso":4,"critico":8}""");

        Assert.Equal(NivelSeveridade.AlertaPreditivo, resultado.Severidade);
        Assert.Equal(5, resultado.ValorObservado);
        Assert.Equal("GRADIENTE_TERMICO_ALERTA", resultado.CodigoRegra);
    }

    [Fact]
    public async Task GradienteTemperaturaRule_NaoDisparaQuandoTemperaturaEstabiliza()
    {
        var contexto = CriarContexto(
            temperatura: 40,
            temperaturaAnterior: 50,
            intervalo: TimeSpan.FromMinutes(1));

        var resultado = await new GradienteTemperaturaRule().AvaliarAsync(
            contexto,
            """{"aviso":4,"critico":8}""");

        Assert.Equal(NivelSeveridade.Normal, resultado.Severidade);
    }

    [Fact]
    public async Task AssimetriaEscovasRule_DisparaCriticoPelaDiferencaEntreEscovas()
    {
        var contexto = CriarContexto(
            temperatura: 70,
            leiturasIrmas:
            [
                new() { NumeroEscova = 1, Temperatura = 70 },
                new() { NumeroEscova = 2, Temperatura = 80 },
                new() { NumeroEscova = 3, Temperatura = 95 }
            ]);

        var resultado = await new AssimetriaEscovasRule().AvaliarAsync(
            contexto,
            """{"aviso":10,"critico":20}""");

        Assert.Equal(NivelSeveridade.Critico, resultado.Severidade);
        Assert.Equal(25, resultado.ValorObservado);
        Assert.Equal("ASSIMETRIA_ESCOVAS_CRITICA", resultado.CodigoRegra);
    }

    [Fact]
    public async Task AssimetriaEscovasRule_RetornaNormalSemComparacao()
    {
        var resultado = await new AssimetriaEscovasRule().AvaliarAsync(
            CriarContexto(temperatura: 70, leiturasIrmas: null),
            """{"aviso":10,"critico":20}""");

        Assert.Equal(NivelSeveridade.Normal, resultado.Severidade);
    }

    private static ContextoAvaliacao CriarContexto(
        double temperatura,
        double? temperaturaAnterior = null,
        TimeSpan? intervalo = null,
        IReadOnlyList<LeituraTelemetriaDto>? leiturasIrmas = null) =>
        new(
            new Sensor { Id = "AA:BB_Escova1", NumeroEscova = 1, MotorId = 12 },
            new LeituraTelemetriaDto { NumeroEscova = 1, Temperatura = temperatura },
            DateTimeOffset.UtcNow,
            leiturasIrmas,
            temperaturaAnterior,
            intervalo);
}
