using Hackaton.Application.DTOs;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;

namespace HackatonApi.Controllers;

public sealed class UsuariosController : BaseController<Usuario, UsuarioDto>
{
    public UsuariosController(IAppService<Usuario, UsuarioDto> service) : base(service) { }
}
