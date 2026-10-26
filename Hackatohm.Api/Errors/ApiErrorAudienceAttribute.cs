namespace Hackatohm.Api.Errors;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiErrorAudienceAttribute(ApiClientAudience audience) : Attribute
{
    public ApiClientAudience Audience { get; } = audience;
}
