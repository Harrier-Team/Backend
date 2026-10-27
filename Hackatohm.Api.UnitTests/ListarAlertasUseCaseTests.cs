using Hackatohm.Api.DTOs;
using Hackatohm.Api.UseCases.Alertas;
using Hackatohm.Api.Validation;
using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Constants;
using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Api.UnitTests;

public sealed class ListarAlertasUseCaseTests
{
    private readonly CapturaRepository repositorio = new();

    private ListarAlertasUseCase CriarUseCase() =>
        new(repositorio, new ListarAlertasRequestDtoValidator());

    [Fact]
    public async Task MapeiaAlertaComCategoriaEEstado()
    {
        var iniciado = new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);
        repositorio.Pagina = new PaginaAlertas(
            [
                new AlertaManutencao
                {
                    Id = 7,
                    MotorId = 3,
                    SensorId = "AA_Escova1",
                    NumeroEscova = 1,
                    NomeRegra = "Tendência",
                    TipoRegra = TiposRegraTemperatura.PrevisaoTendencia,
                    CodigoRegra = "PREVISAO_TENDENCIA_ALERTA",
                    Severidade = NivelSeveridade.AlertaPreditivo,
                    ValorObservado = 8,
                    ValorLimite = 10,
                    Mensagem = "tende a atingir",
                    IniciadoEm = iniciado,
                    AtualizadoEm = iniciado
                }
            ],
            Total: 41);

        var resposta = await CriarUseCase().ExecutarAsync(new ListarAlertasRequestDto { Pagina = 2, Tamanho = 10 });

        Assert.Equal(2, resposta.Pagina);
        Assert.Equal(10, resposta.Tamanho);
        Assert.Equal(41, resposta.Total);
        var item = Assert.Single(resposta.Itens);
        Assert.Equal(CategoriaAlerta.Preditivo, item.Categoria);
        Assert.True(item.Ativo);
        Assert.Null(item.ResolvidoEm);
        Assert.Equal((2, 10), (repositorio.PaginaSolicitada, repositorio.Tamanho));
    }

    [Fact]
    public async Task CategoriaVirafiltroPorTiposDeRegra()
    {
        await CriarUseCase().ExecutarAsync(new ListarAlertasRequestDto { Categoria = CategoriaAlerta.Condicao });

        Assert.Equivalent(
            new[] { TiposRegraTemperatura.LimiarEstatico, TiposRegraTemperatura.AssimetriaEscovas },
            repositorio.Filtro!.TiposRegra);
    }

    [Fact]
    public async Task CategoriaETipoIncompativeisNaoRetornamNada()
    {
        await CriarUseCase().ExecutarAsync(new ListarAlertasRequestDto
        {
            Categoria = CategoriaAlerta.Preditivo,
            TipoRegra = TiposRegraTemperatura.LimiarEstatico
        });

        Assert.Empty(repositorio.Filtro!.TiposRegra!);
    }

    [Fact]
    public async Task SemCategoriaNemTipoNaoFiltraPorRegra()
    {
        await CriarUseCase().ExecutarAsync(new ListarAlertasRequestDto { MotorId = 3, Ativo = true });

        Assert.Null(repositorio.Filtro!.TiposRegra);
        Assert.Equal(3, repositorio.Filtro.MotorId);
        Assert.True(repositorio.Filtro.Ativo);
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 0)]
    [InlineData(1, 201)]
    public async Task RejeitaPaginacaoInvalida(int pagina, int tamanho)
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () =>
            await CriarUseCase().ExecutarAsync(new ListarAlertasRequestDto { Pagina = pagina, Tamanho = tamanho }));
    }

    [Fact]
    public async Task RejeitaMotorInvalido()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () =>
            await CriarUseCase().ExecutarAsync(new ListarAlertasRequestDto { MotorId = 0 }));
    }

    [Fact]
    public void CategoriaDeTipoRegraClassificaOsTiposConhecidos()
    {
        Assert.Equal(CategoriaAlerta.Preditivo, CategoriaAlertaExtensions.DeTipoRegra(TiposRegraTemperatura.PrevisaoTendencia));
        Assert.Equal(CategoriaAlerta.Aceleracao, CategoriaAlertaExtensions.DeTipoRegra(TiposRegraTemperatura.GradienteTermico));
        Assert.Equal(CategoriaAlerta.Condicao, CategoriaAlertaExtensions.DeTipoRegra(TiposRegraTemperatura.LimiarEstatico));
        Assert.Equal(CategoriaAlerta.Condicao, CategoriaAlertaExtensions.DeTipoRegra(TiposRegraTemperatura.AssimetriaEscovas));
    }

    private sealed class CapturaRepository : IAlertaManutencaoRepository
    {
        public PaginaAlertas Pagina { get; set; } = new([], 0);
        public FiltroAlertas? Filtro { get; private set; }
        public int PaginaSolicitada { get; private set; }
        public int Tamanho { get; private set; }

        public Task<PaginaAlertas> ListarAsync(
            FiltroAlertas filtro, int pagina, int tamanho, CancellationToken cancellationToken = default)
        {
            Filtro = filtro;
            PaginaSolicitada = pagina;
            Tamanho = tamanho;
            return Task.FromResult(Pagina);
        }

        public Task<IReadOnlyList<AlertaManutencao>> ObterAbertosPorMotorAsync(
            int motorId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task AdicionarRangeAsync(
            IEnumerable<AlertaManutencao> alertas, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
