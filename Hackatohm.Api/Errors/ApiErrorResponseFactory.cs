using Hackatohm.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Hackatohm.Api.Errors;

public static class ApiErrorResponseFactory
{
    public static ObjectResult ToProblemResult(
        HttpContext context,
        ApiErrorDefinition error) =>
        new(CreateProblemDetails(context, error))
        {
            StatusCode = error.StatusCode
        };

    public static bool IsDeviceRequest(HttpContext context) =>
        context.GetEndpoint()?.Metadata.GetMetadata<ApiErrorAudienceAttribute>()?.Audience
        == ApiClientAudience.Device;

    public static DispositivoErroResponseDto CreateDeviceError(
        HttpContext context,
        ApiErrorDefinition error) =>
        new(false, error.Code, context.TraceIdentifier);

    public static ProblemDetails CreateProblemDetails(
        HttpContext context,
        ApiErrorDefinition error)
    {
        var problem = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Detail,
            Instance = context.Request.Path
        };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        return problem;
    }

    public static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        IDictionary<string, string[]> errors)
    {
        var error = ApiErrorCatalog.InvalidPayload;
        var problem = new ValidationProblemDetails(errors)
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Detail,
            Instance = context.Request.Path
        };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        return problem;
    }
}
