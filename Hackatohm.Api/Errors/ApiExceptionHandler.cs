using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace Hackatohm.Api.Errors;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.RequestAborted.IsCancellationRequested)
            return true;

        if (httpContext.Response.HasStarted)
            return false;

        if (exception is ValidationException validationException)
        {
            var validationErrors = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

            logger.LogWarning(
                "Validação da requisição falhou. TraceId: {TraceId}, Path: {Path}, Erros: {ValidationErrors}.",
                httpContext.TraceIdentifier,
                httpContext.Request.Path,
                validationErrors);

            await EscreverErroAsync(
                httpContext,
                ApiErrorCatalog.InvalidPayload,
                validationErrors,
                cancellationToken);
            return true;
        }

        logger.LogError(
            exception,
            "Falha inesperada ao processar a requisição. TraceId: {TraceId}, Path: {Path}, Method: {Method}.",
            httpContext.TraceIdentifier,
            httpContext.Request.Path,
            httpContext.Request.Method);

        await EscreverErroAsync(
            httpContext,
            ApiErrorCatalog.InternalError,
            null,
            cancellationToken);
        return true;
    }

    private static async Task EscreverErroAsync(
        HttpContext context,
        ApiErrorDefinition error,
        IDictionary<string, string[]>? validationErrors,
        CancellationToken cancellationToken)
    {
        context.Response.StatusCode = error.StatusCode;

        if (ApiErrorResponseFactory.IsDeviceRequest(context))
        {
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                ApiErrorResponseFactory.CreateDeviceError(context, error),
                JsonOptions,
                cancellationToken: cancellationToken);
            return;
        }

        if (validationErrors is not null)
        {
            context.Response.ContentType = "application/problem+json";
            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                ApiErrorResponseFactory.CreateValidationProblemDetails(context, validationErrors),
                JsonOptions,
                cancellationToken: cancellationToken);
            return;
        }

        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            ApiErrorResponseFactory.CreateProblemDetails(context, error),
            JsonOptions,
            cancellationToken: cancellationToken);
    }
}
