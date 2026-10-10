using FluentValidation;
using Hackaton.Application.DTOs;

namespace Hackaton.Application.Validators;

public sealed class UsuarioDtoValidator : AbstractValidator<UsuarioDto>
{
    public UsuarioDtoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().Length(2, 160);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Telefone).MaximumLength(30);
        RuleFor(x => x.PrefeituraId).NotEmpty();
        RuleFor(x => x.Papel).Must(x => x is "Usuario" or "Tecnico" or "Admin");
    }
}
