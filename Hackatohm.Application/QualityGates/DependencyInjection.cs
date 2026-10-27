using Hackatohm.Domain.QualityGates.Rules;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Application.MotorStatus;
using Microsoft.Extensions.DependencyInjection;

namespace Hackatohm.Application.QualityGates;

public static class DependencyInjection
{
    public static IServiceCollection AddQualityGates(this IServiceCollection services)
    {
        services.AddScoped<IQualityGateRule, LimiarTemperaturaRule>();
        services.AddScoped<IQualityGateRule, GradienteTemperaturaRule>();
        services.AddScoped<IQualityGateRule, AssimetriaEscovasRule>();
        services.AddScoped<IQualityGateEvaluator, QualityGateEvaluator>();
        services.AddScoped<IMotorStatusService, MotorStatusService>();

        return services;
    }
}
