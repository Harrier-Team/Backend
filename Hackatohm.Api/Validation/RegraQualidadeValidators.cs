using FluentValidation;
using Hackatohm.Api.DTOs;
using Hackatohm.Domain.QualityGates.Constants;
using Hackatohm.Domain.QualityGates.Rules;

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
            .WithMessage("O tipo de regra deve ser limiar estático, gradiente térmico, assimetria entre escovas ou previsão por tendência.");

        RuleFor(request => request.Parametros)
            .NotNull().WithMessage("Os parâmetros da regra são obrigatórios.")
            .Must((request, parametros) => ParametrosValidos(request.TipoRegra, parametros))
            .WithMessage("Os parâmetros devem ser finitos e respeitar os limites do tipo de regra: aviso menor que crítico; na previsão por tendência, aviso e crítico são minutos (aviso maior que crítico) e a temperatura limite é obrigatória.");
    }

    private static bool TiposRegraSuportados(string tipoRegra) =>
        tipoRegra is TiposRegraTemperatura.LimiarEstatico
            or TiposRegraTemperatura.GradienteTermico
            or TiposRegraTemperatura.AssimetriaEscovas
            or TiposRegraTemperatura.PrevisaoTendencia;

    private static bool ParametrosValidos(
        string tipoRegra,
        ParametrosLimiarTemperaturaDto? parametros) =>
        parametros is not null &&
        double.IsFinite(parametros.Aviso) &&
        double.IsFinite(parametros.Critico) &&
        (tipoRegra == TiposRegraTemperatura.PrevisaoTendencia
            ? ParametrosTendenciaValidos(parametros)
            : ParametrosLimiarValidos(tipoRegra, parametros));

    private static bool ParametrosTendenciaValidos(ParametrosLimiarTemperaturaDto parametros) =>
        parametros.TemperaturaLimite is { } limite &&
        double.IsFinite(limite) &&
        limite >= -55 &&
        limite <= 200 &&
        parametros.Critico > 0 &&
        parametros.Aviso > parametros.Critico &&
        (parametros.Janela is null ||
         parametros.Janela is >= ParametrosPrevisaoTendencia.JanelaMinima
             and <= ParametrosPrevisaoTendencia.JanelaMaxima);

    private static bool ParametrosLimiarValidos(
        string tipoRegra,
        ParametrosLimiarTemperaturaDto parametros) =>
        parametros.TemperaturaLimite is null &&
        parametros.Janela is null &&
        (tipoRegra == TiposRegraTemperatura.LimiarEstatico
            ? parametros.Aviso >= -55 &&
              parametros.Aviso <= 200 &&
              parametros.Critico >= -55 &&
              parametros.Critico <= 200
            : parametros.Aviso >= 0 &&
              parametros.Critico >= 0) &&
        parametros.Aviso < parametros.Critico;
}
