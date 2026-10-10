using Hackaton.Application.DTOs;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;

namespace HackatonApi.Controllers;

public sealed class ChamadosController : BaseController<Chamado, ChamadoDto>
{
    public ChamadosController(IAppService<Chamado, ChamadoDto> service) : base(service) { }
}
