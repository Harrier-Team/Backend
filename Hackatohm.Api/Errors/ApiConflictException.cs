namespace Hackatohm.Api.Errors;

public sealed class ApiConflictException(
    ApiErrorDefinition error,
    Exception? innerException = null) : Exception(error.Detail, innerException)
{
    public ApiErrorDefinition Error { get; } = error;
}
