using Hackaton.Application.DTOs;
using Hackaton.Domain.Entities;
using Mapster;

namespace Hackaton.Application.Mappings;

public sealed class UsuarioMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UsuarioDto, Usuario>().Ignore(x => x.Id);
        config.NewConfig<Usuario, UsuarioDto>();
    }
}

public sealed class UnidadeAtendimentoMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UnidadeAtendimentoDto, UnidadeAtendimento>().Ignore(x => x.Id);
        config.NewConfig<UnidadeAtendimento, UnidadeAtendimentoDto>();
    }
}

public sealed class ChamadoMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ChamadoDto, Chamado>()
            .Ignore(x => x.Id)
            .Ignore(x => x.Status)
            .Ignore(x => x.TecnicoResponsavelId)
            .Ignore(x => x.InicioAtendimento);
        config.NewConfig<Chamado, ChamadoDto>();
    }
}
