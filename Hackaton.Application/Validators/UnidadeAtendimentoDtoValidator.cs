using FluentValidation;
using Hackaton.Application.DTOs;

namespace Hackaton.Application.Validators;

public sealed class UnidadeAtendimentoDtoValidator : AbstractValidator<UnidadeAtendimentoDto>
{
    public UnidadeAtendimentoDtoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().Length(2, 160);
        RuleFor(x => x.PrefeituraId).NotEmpty();
        RuleFor(x => x.Endereco).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Latitude).NotNull().WithMessage("Informe a latitude da unidade.");
        RuleFor(x => x.Longitude).NotNull().WithMessage("Informe a longitude da unidade.");
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x).Must(x => x.Latitude.HasValue == x.Longitude.HasValue)
            .WithMessage("Latitude e longitude devem ser informadas juntas.");
    }
}
