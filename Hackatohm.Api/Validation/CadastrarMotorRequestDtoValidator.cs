using FluentValidation;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.Entities;

namespace Hackatohm.Api.Validation;

public sealed class CadastrarMotorRequestDtoValidator
    : AbstractValidator<CadastrarMotorRequestDto>
{
    public CadastrarMotorRequestDtoValidator()
    {
        RuleFor(request => request.Nome)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O nome do motor é obrigatório.")
            .MaximumLength(100)
            .WithMessage("O nome do motor deve ter no máximo 100 caracteres.");

        RuleFor(request => request.Tipo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O tipo do motor é obrigatório.")
            .MaximumLength(80)
            .WithMessage("O tipo do motor deve ter no máximo 80 caracteres.");

        RuleFor(request => request.SensoresIniciais)
            .NotNull()
            .WithMessage("A lista de sensores iniciais é obrigatória.")
            .NotEmpty()
            .WithMessage("Ao menos um sensor inicial deve ser associado ao motor.");

        RuleFor(request => request.SensoresIniciais)
            .Must(sensores =>
                sensores!.All(sensor => sensor is not null) &&
                sensores.Select(sensor => Sensor.CriarId(sensor.MacAddress, sensor.NumeroEscova))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count() == sensores.Count)
            .When(request => request.SensoresIniciais is not null)
            .WithMessage("Há sensores duplicados no payload da requisição.");

        RuleForEach(request => request.SensoresIniciais)
            .SetValidator(new SensorInicialMotorRequestDtoValidator());
    }
}

public sealed class SensorInicialMotorRequestDtoValidator
    : AbstractValidator<SensorInicialMotorRequestDto>
{
    public SensorInicialMotorRequestDtoValidator()
    {
        RuleFor(sensor => sensor.MacAddress)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O endereço MAC do sensor é obrigatório.")
            .MaximumLength(47)
            .WithMessage("O endereço MAC excede o tamanho permitido para o identificador do sensor.");

        RuleFor(sensor => sensor.Modelo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O modelo do sensor é obrigatório.")
            .MaximumLength(80)
            .WithMessage("O modelo do sensor deve ter no máximo 80 caracteres.");

        RuleFor(sensor => sensor.NumeroEscova)
            .GreaterThan(0)
            .WithMessage("O número da escova deve ser positivo.");
    }
}
