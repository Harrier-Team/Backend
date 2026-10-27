using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.DTOs.Hubs;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates.Enums;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Rules;
using Hackatohm.Api.Validation;

namespace Hackatohm.Api.UnitTests;

public sealed class QualityGateContractTests
{
    [Theory]
    [InlineData(80, 95)]
    [InlineData(-55, 125)]
    [InlineData(-55, 200)]
    [InlineData(126, 200)]
    public void RegraValidador_AceitaLimitesFinitos(double aviso, double critico)
    {
        var validator = new CadastrarRegraQualidadeRequestDtoValidator();
        var request = new CadastrarRegraQualidadeRequestDto
        {
            Nome = "Limiar térmico",
            Parametros = new ParametrosLimiarTemperaturaDto(aviso, critico)
        };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
        var editResult = new EditarRegraQualidadeRequestDtoValidator().Validate(
            new EditarRegraQualidadeRequestDto
            {
                Nome = request.Nome, Parametros = request.Parametros, TipoRegra = request.TipoRegra
            });
        Assert.True(editResult.IsValid);
    }

    [Theory]
    [InlineData(double.NaN, 95)]
    [InlineData(double.PositiveInfinity, 95)]
    [InlineData(95, 80)]
    [InlineData(-56, 95)]
    [InlineData(80, 201)]
    public void RegraValidador_RejeitaLimitesInvalidos(double aviso, double critico)
    {
        var validator = new CadastrarRegraQualidadeRequestDtoValidator();
        var request = new CadastrarRegraQualidadeRequestDto
        {
            Nome = "Limiar térmico",
            Parametros = new ParametrosLimiarTemperaturaDto(aviso, critico)
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Parametros");
    }

    [Fact]
    public async Task LimiarTemperaturaRule_ProduzResultadoCritico()
    {
        var rule = new LimiarTemperaturaRule();
        var contexto = CriarContexto(96);

        var result = await rule.AvaliarAsync(contexto, """{"aviso":80,"critico":95}""");

        Assert.True(result.DisparouAlerta);
        Assert.Equal(NivelSeveridade.Critico, result.Severidade);
        Assert.Equal("TEMP_CRITICA", result.CodigoRegra);
        Assert.Equal(96, result.ValorObservado);
        Assert.Equal(95, result.ValorLimite);
    }

    [Fact]
    public async Task LimiarTemperaturaRule_ProduzResultadoNormal()
    {
        var rule = new LimiarTemperaturaRule();

        var result = await rule.AvaliarAsync(CriarContexto(75), """{"aviso":80,"critico":95}""");

        Assert.False(result.DisparouAlerta);
        Assert.Equal(NivelSeveridade.Normal, result.Severidade);
        Assert.Null(result.ValorObservado);
        Assert.Null(result.ValorLimite);
    }

    [Fact]
    public void TelemetriaRealtimeDto_SerializaContratoCanônico()
    {
        var dto = new TelemetriaRealtimeDto(
            "AA:BB_Escova1",
            "AA:BB",
            1,
            82.5,
            new DateTimeOffset(2026, 10, 25, 20, 10, 0, TimeSpan.FromHours(-3)),
            true,
            null,
            []);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));

        var json = JsonSerializer.Serialize(dto, options);

        Assert.Contains("\"sensorId\":\"AA:BB_Escova1\"", json);
        Assert.Contains("\"dataHora\":\"2026-10-25T20:10:00-03:00\"", json);
        Assert.Contains("\"motorId\":null", json);
        Assert.Contains("\"resultadosQualityGates\":[]", json);
    }

    [Fact]
    public void NivelSeveridade_SerializaTodosOsValoresComoString()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));

        var json = JsonSerializer.Serialize(
            Enum.GetValues<NivelSeveridade>(),
            options);

        Assert.Equal("""["Normal","Atencao","AlertaPreditivo","Critico"]""", json);
    }

    [Fact]
    public void NivelSeveridade_NaoAceitaRepresentacaoNumerica()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<NivelSeveridade>("0", options));
    }

    [Fact]
    public void ResultadoGateRealtime_SerializaValoresOpcionaisNulos()
    {
        var dto = new ResultadoGateRealtimeDto(
            Guid.NewGuid(),
            "Limiar térmico",
            false,
            NivelSeveridade.Normal,
            "temperatura.limiar_estatico",
            "OK",
            "Operação dentro dos padrões normais.",
            null,
            null,
            null);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));

        var json = JsonSerializer.Serialize(dto, options);

        Assert.Contains("\"valorObservado\":null", json);
        Assert.Contains("\"valorLimite\":null", json);
        Assert.Contains("\"metadados\":null", json);
    }

    private static ContextoAvaliacao CriarContexto(double temperatura) =>
        new(
            new Sensor
            {
                Id = "AA:BB_Escova1",
                NumeroEscova = 1,
                MotorId = 12
            },
            new LeituraTelemetriaDto
            {
                NumeroEscova = 1,
                Temperatura = temperatura
            },
            DateTimeOffset.UtcNow);
}
