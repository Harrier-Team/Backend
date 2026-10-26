using FluentValidation;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.Validation;

public sealed class ReceberTelemetriaRequestDtoValidator : AbstractValidator<ReceberTelemetriaRequestDto>
{
    public ReceberTelemetriaRequestDtoValidator()
    {
        RuleFor(payload => payload.EspMacAddress)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O endereço MAC do ESP é obrigatório.");

        RuleFor(payload => payload.Sensores)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("A lista de sensores é obrigatória.")
            .NotEmpty()
            .WithMessage("O lote deve conter pelo menos uma leitura.")
            .Must(sensores =>
                sensores!.All(leitura => leitura is not null) &&
                sensores.Select(leitura => leitura.NumeroEscova).Distinct().Count() == sensores.Count)
            .WithMessage("O lote contém leituras nulas ou números de escova duplicados.");

        RuleForEach(payload => payload.Sensores)
            .SetValidator(new LeituraTelemetriaDtoValidator());
    }
}

public sealed class LeituraTelemetriaDtoValidator : AbstractValidator<LeituraTelemetriaDto>
{
    public LeituraTelemetriaDtoValidator()
    {
        RuleFor(leitura => leitura.NumeroEscova)
            .GreaterThan(0)
            .WithMessage("O número da escova deve ser positivo.");

        RuleFor(leitura => leitura.Temperatura)
            .Cascade(CascadeMode.Stop)
            .Must(double.IsFinite)
            .WithMessage("A temperatura deve ser um número finito.")
            .InclusiveBetween(-55, 125)
            .WithMessage("A temperatura deve estar entre -55 °C e 125 °C.");
    }
}
