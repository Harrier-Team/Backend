using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.Data.Repositories;

namespace Hackatohm.Api.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<IMotorRepository, MotorRepository>();
        services.AddScoped<ILeituraTemperaturaRepository, LeituraTemperaturaRepository>();
        services.AddScoped<IRegraQualidadeRepository, RegraQualidadeRepository>();
        services.AddScoped<IMotorRegraQualidadeRepository, MotorRegraQualidadeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
