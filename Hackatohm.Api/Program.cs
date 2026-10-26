using Hackatohm.Infrastructure.Persistence;
using Hackatohm.Application.QualityGates;
using Hackatohm.Api.Errors;
using Hackatohm.Api.Hubs;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Api.Queues;
using Hackatohm.Api.Observability;
using Hackatohm.Api.UseCases;
using Hackatohm.Api.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = DatabaseConnectionString.GetRequired(
    builder.Configuration,
    builder.Environment.ContentRootPath);

// Configuração de CORS (Obrigatório para o SignalR aceitar conexões do navegador)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Registra o serviço do SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
}).AddJsonProtocol(options =>
    options.PayloadSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services
    .AddOptions<TelemetriaOptions>()
    .Bind(builder.Configuration.GetSection(TelemetriaOptions.SectionName))
    .Validate(settings => settings.QueueCapacity > 0, "A capacidade da fila deve ser positiva.")
    .Validate(settings => settings.MaxBatchSize > 0, "O tamanho máximo do lote deve ser positivo.")
    .Validate(settings => settings.OfflineAfterSeconds > 0, "O tempo para considerar o motor offline deve ser positivo.")
    .ValidateOnStart();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false)))
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            if (ApiErrorResponseFactory.IsDeviceRequest(context.HttpContext))
            {
                return new ObjectResult(ApiErrorResponseFactory.CreateDeviceError(
                    context.HttpContext,
                    ApiErrorCatalog.InvalidPayload))
                {
                    StatusCode = ApiErrorCatalog.InvalidPayload.StatusCode
                };
            }

            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "Valor inválido."
                            : error.ErrorMessage)
                        .ToArray());

            return new ObjectResult(ApiErrorResponseFactory.CreateValidationProblemDetails(
                context.HttpContext,
                errors))
            {
                StatusCode = ApiErrorCatalog.InvalidPayload.StatusCode
            };
        };
    });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("postgresql", tags: ["ready"])
    .AddCheck<TelemetriaQueueHealthCheck>("telemetria_queue", tags: ["live", "ready"]);

builder.Services.AddSingleton<TelemetriaQueue>();
builder.Services.AddHostedService<TelemetriaWorker>();
builder.Services.AddRepositories();
builder.Services.AddQualityGates();
builder.Services.AddUseCases();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
// Mapeia o Hub de WebSockets
app.MapHub<TelemetriaHub>("/api/telemetriaHub");

app.Run();
