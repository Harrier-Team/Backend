using FluentValidation;
using Hackatohm.Api.DTOs;

namespace Hackatohm.Api.Validation;

public sealed class ListarAlertasRequestDtoValidator : AbstractValidator<ListarAlertasRequestDto>
{
    public ListarAlertasRequestDtoValidator()
    {
        RuleFor(request => request.MotorId)
            .GreaterThan(0).When(request => request.MotorId is not null)
            .WithMessage("O identificador do motor deve ser positivo.");

        RuleFor(request => request.Categoria)
            .IsInEnum().When(request => request.Categoria is not null)
            .WithMessage("A categoria deve ser Preditivo, Aceleracao ou Condicao.");

        RuleFor(request => request.Severidade)
            .IsInEnum().When(request => request.Severidade is not null)
            .WithMessage("A severidade informada não é válida.");

        RuleFor(request => request.TipoRegra)
            .MaximumLength(100)
            .WithMessage("O tipo de regra deve ter no máximo 100 caracteres.");

        RuleFor(request => request.Pagina)
            .GreaterThanOrEqualTo(1)
            .WithMessage("A página deve começar em 1.");

        RuleFor(request => request.Tamanho)
            .InclusiveBetween(1, ListarAlertasRequestDto.TamanhoMaximo)
            .WithMessage($"O tamanho da página deve estar entre 1 e {ListarAlertasRequestDto.TamanhoMaximo}.");
    }
}
