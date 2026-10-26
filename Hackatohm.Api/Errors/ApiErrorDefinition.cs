namespace Hackatohm.Api.Errors;

public sealed record ApiErrorDefinition(
    string Code,
    int StatusCode,
    string Title,
    string Detail);
