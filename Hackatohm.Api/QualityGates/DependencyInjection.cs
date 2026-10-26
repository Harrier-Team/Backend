using Hackatohm.Domain.QualityGates.Rules;

namespace Hackatohm.Domain.QualityGates;

public static class DependencyInjection
{
    public static IServiceCollection AddQualityGates(this IServiceCollection services)
    {
        services.AddScoped<IQualityGateRule, LimiarTemperaturaRule>();
        services.AddScoped<IQualityGateEvaluator, QualityGateEvaluator>();

        return services;
    }
}
