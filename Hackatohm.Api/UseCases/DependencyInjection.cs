using FluentValidation;
using Hackatohm.Api.Validation;
using Hackatohm.Api.UseCases.Leituras;
using Hackatohm.Api.UseCases.Motores;
using Hackatohm.Api.UseCases.RegrasQualidade;
using Hackatohm.Api.UseCases.Sensores;
using Hackatohm.Api.Observability;

namespace Hackatohm.Api.UseCases;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ProcessarLeiturasLoteUseCase>();
        services.AddSingleton<TelemetriaMetrics>();
        services.AddScoped<ReceberTelemetriaUseCase>();
        services.AddScoped<ObterUltimaLeituraUseCase>();
        services.AddScoped<CadastrarMotorUseCase>();
        services.AddScoped<ObterMotorUseCase>();
        services.AddScoped<ListarMotoresUseCase>();
        services.AddScoped<DeletarMotorUseCase>();
        services.AddScoped<CadastrarSensorUseCase>();
        services.AddScoped<ObterSensorUseCase>();
        services.AddScoped<PingSensorUseCase>();
        services.AddScoped<CadastrarRegraQualidadeUseCase>();
        services.AddScoped<ListarRegrasQualidadeUseCase>();
        services.AddScoped<ObterRegraQualidadeUseCase>();
        services.AddScoped<EditarRegraQualidadeUseCase>();
        services.AddScoped<ExcluirRegraQualidadeUseCase>();
        services.AddScoped<VincularRegraQualidadeMotorUseCase>();
        services.AddScoped<ListarRegrasQualidadeMotorUseCase>();
        services.AddScoped<ObterVinculoRegraQualidadeUseCase>();
        services.AddScoped<AtualizarVinculoRegraQualidadeUseCase>();
        services.AddScoped<DesvincularRegraQualidadeMotorUseCase>();
        services.AddValidatorsFromAssemblyContaining<ReceberTelemetriaRequestDtoValidator>();

        return services;
    }
}
