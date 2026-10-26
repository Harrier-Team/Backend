using FluentValidation;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.Validation;

public sealed class ObterUltimaLeituraRequestDtoValidator
    : AbstractValidator<ObterUltimaLeituraRequestDto>
{
    public ObterUltimaLeituraRequestDtoValidator()
    {
        RuleFor(request => request.EspMacAddress)
            .NotEmpty()
            .WithMessage("O endereço MAC do ESP é obrigatório.");

        RuleFor(request => request.NumeroEscova)
            .GreaterThan(0)
            .WithMessage("O número da escova deve ser positivo.");
    }
}
