using FluentValidation;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.QualityGates.Constants;

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

        RuleFor(request => request.TipoRegra)
            .Must(TiposRegraSuportados)
            .WithMessage("O tipo de regra deve ser limiar estático, gradiente térmico ou assimetria entre escovas.");

        RuleFor(request => request.Parametros)
            .NotNull().WithMessage("Os parâmetros da regra são obrigatórios.")
            .Must((request, parametros) => ParametrosValidos(request.TipoRegra, parametros))
            .WithMessage("Os parâmetros devem ser finitos, ter aviso menor que crítico e respeitar os limites do tipo de regra.");
    }

    private static bool TiposRegraSuportados(string tipoRegra) =>
        tipoRegra is TiposRegraTemperatura.LimiarEstatico
            or TiposRegraTemperatura.GradienteTermico
            or TiposRegraTemperatura.AssimetriaEscovas;

    private static bool ParametrosValidos(
        string tipoRegra,
        ParametrosLimiarTemperaturaDto? parametros) =>
        parametros is not null &&
        double.IsFinite(parametros.Aviso) &&
        double.IsFinite(parametros.Critico) &&
        (tipoRegra == TiposRegraTemperatura.LimiarEstatico
            ? parametros.Aviso >= -55 &&
              parametros.Aviso <= 200 &&
              parametros.Critico >= -55 &&
              parametros.Critico <= 200
            : parametros.Aviso >= 0 &&
              parametros.Critico >= 0) &&
        parametros.Aviso < parametros.Critico;
}
