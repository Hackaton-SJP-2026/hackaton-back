using Hackaton.Application.DTOs;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;

namespace HackatonApi.Controllers;

public sealed class UnidadesAtendimentoController : BaseController<UnidadeAtendimento, UnidadeAtendimentoDto>
{
    public UnidadesAtendimentoController(IAppService<UnidadeAtendimento, UnidadeAtendimentoDto> service) : base(service) { }
}
