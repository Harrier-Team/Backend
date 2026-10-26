using FluentValidation;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.Validation;

public sealed class CadastrarSensorRequestDtoValidator : AbstractValidator<CadastrarSensorRequestDto>
{
    public CadastrarSensorRequestDtoValidator()
    {
        RuleFor(dto => dto.MacAddress)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O endereço MAC é obrigatório.")
            .MaximumLength(47)
            .WithMessage("O endereço MAC excede o tamanho permitido para o identificador do sensor.");

        RuleFor(dto => dto.Modelo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O modelo do sensor é obrigatório.")
            .MaximumLength(80)
            .WithMessage("O modelo do sensor deve ter no máximo 80 caracteres.");

        RuleFor(dto => dto.NumeroEscova)
            .GreaterThan(0)
            .WithMessage("O número da escova deve ser positivo.");

        RuleFor(dto => dto.MotorId)
            .GreaterThan(0)
            .WithMessage("O ID do motor deve ser positivo.");
    }
}
