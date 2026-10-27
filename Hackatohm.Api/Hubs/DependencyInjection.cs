using System.Text.Json.Serialization;

namespace Hackatohm.Api.Hubs;

public static class DependencyInjection
{
    public static IServiceCollection AddTelemetriaSignalR(
        this IServiceCollection services, bool detailedErrors)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = detailedErrors;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        }).AddJsonProtocol(options =>
            options.PayloadSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(allowIntegerValues: false)));
        return services;
    }
}
