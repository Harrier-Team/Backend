using FluentValidation;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.Validation;

public sealed class CadastrarRegraQualidadeRequestDtoValidator
    : RegraQualidadeRequestValidator<CadastrarRegraQualidadeRequestDto>
{
}

public sealed class EditarRegraQualidadeRequestDtoValidator
    : RegraQualidadeRequestValidator<EditarRegraQualidadeRequestDto>
{
}

public abstract class RegraQualidadeRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : IRegraQualidadeRequestDto
{
    protected RegraQualidadeRequestValidator()
    {
        RuleFor(request => request.Nome)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O nome da regra é obrigatório.")
            .MaximumLength(120).WithMessage("O nome da regra deve ter no máximo 120 caracteres.");

        RuleFor(request => request.Descricao)
            .MaximumLength(255).WithMessage("A descrição deve ter no máximo 255 caracteres.");

        RuleFor(request => request.Parametros)
            .NotNull().WithMessage("Os parâmetros da regra são obrigatórios.")
            .Must(ParametrosValidos)
            .WithMessage("Os parâmetros devem ser finitos, estar entre -55 °C e 125 °C, e o aviso deve ser menor que o limite crítico.");
    }

    private static bool ParametrosValidos(ParametrosLimiarTemperaturaDto? parametros) =>
        parametros is not null &&
        double.IsFinite(parametros.Aviso) &&
        double.IsFinite(parametros.Critico) &&
        parametros.Aviso >= -55 &&
        parametros.Aviso <= 125 &&
        parametros.Critico >= -55 &&
        parametros.Critico <= 125 &&
        parametros.Aviso < parametros.Critico;
}
