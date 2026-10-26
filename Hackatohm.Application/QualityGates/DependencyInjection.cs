using Hackatohm.Domain.QualityGates.Rules;
using Hackatohm.Domain.QualityGates;
using Microsoft.Extensions.DependencyInjection;

namespace Hackatohm.Application.QualityGates;

public static class DependencyInjection
{
    public static IServiceCollection AddQualityGates(this IServiceCollection services)
    {
        services.AddScoped<IQualityGateRule, LimiarTemperaturaRule>();
        services.AddScoped<IQualityGateEvaluator, QualityGateEvaluator>();

        return services;
    }
}
