using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Hackatohm.Infrastructure.Persistence;

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
