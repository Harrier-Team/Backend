using Hackatohm.Api.QualityGates.Rules;

namespace Hackatohm.Api.QualityGates;

public static class DependencyInjection
{
    public static IServiceCollection AddQualityGates(this IServiceCollection services)
    {
        services.AddScoped<IQualityGateRule, LimiarTemperaturaRule>();
        services.AddScoped<IQualityGateEvaluator, QualityGateEvaluator>();

        return services;
    }
}
