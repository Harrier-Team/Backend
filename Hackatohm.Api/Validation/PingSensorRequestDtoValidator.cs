using FluentValidation;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.Validation;

public sealed class PingSensorRequestDtoValidator : AbstractValidator<PingSensorRequestDto>
{
    public PingSensorRequestDtoValidator()
    {
        RuleFor(request => request.SensorId)
            .NotEmpty()
            .WithMessage("O ID do sensor é obrigatório.");
    }
}
