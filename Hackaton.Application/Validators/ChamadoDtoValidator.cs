using FluentValidation;
using Hackaton.Application.DTOs;

namespace Hackaton.Application.Validators;

public sealed class ChamadoDtoValidator : AbstractValidator<ChamadoDto>
{
    public ChamadoDtoValidator()
    {
        RuleFor(x => x.Descricao).NotEmpty().Length(5, 4000);
        RuleFor(x => x.SolicitanteId).NotEmpty();
        RuleFor(x => x.UnidadeAtendimentoId).NotEmpty();
        RuleFor(x => x.AreaAtendimento).Must(x => x is "Suporte" or "Telecom" or "TI" or "TI / Sistemas");
        RuleFor(x => x.Natureza).Must(x => x is "Hardware" or "Software");
        RuleFor(x => x.Modalidade).Must(x => x is "Remoto" or "Presencial");
        RuleFor(x => x.Prioridade).Must(x => x is "Baixa" or "Normal" or "Alta" or "Urgente");
        RuleFor(x => x.Equipamento).MaximumLength(160);
        RuleFor(x => x.Contato).MaximumLength(100);
        RuleFor(x => x.PessoasAfetadas).GreaterThan(0);
        RuleFor(x => x.LocalRetiradaMaterial).MaximumLength(300);
        RuleFor(x => x.LocalRetiradaMaterial).NotEmpty()
            .When(x => x.NecessitaMaterial).WithMessage("Informe o local de retirada do material.");
    }
}
