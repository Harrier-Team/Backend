using FluentValidation;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.Validation;

public sealed class ObterSensorRequestDtoValidator : AbstractValidator<ObterSensorRequestDto>
{
    public ObterSensorRequestDtoValidator()
    {
        RuleFor(request => request.SensorId)
            .NotEmpty()
            .WithMessage("O ID do sensor é obrigatório.")
            .MaximumLength(64)
            .WithMessage("O ID do sensor deve ter no máximo 64 caracteres.");
    }
}
