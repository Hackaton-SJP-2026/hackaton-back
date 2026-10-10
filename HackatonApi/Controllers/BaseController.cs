using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;

namespace HackatonApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController<TEntity, TDto> : ControllerBase where TEntity : Entity
    {
        protected readonly IAppService<TEntity, TDto> _service;

        protected BaseController(IAppService<TEntity, TDto> service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Gestor,Admin")]
        public virtual async Task<IActionResult> Get(CancellationToken cancellationToken)
            => Ok(await _service.GetAllAsync(cancellationToken));

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Gestor,Admin")]
        public virtual async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var dto = await _service.GetAsync(id, cancellationToken);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Gestor,Admin")]
        public virtual async Task<IActionResult> Post([FromBody] TDto dto, CancellationToken cancellationToken)
            => Ok(await _service.CreateAsync(dto, cancellationToken));

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Gestor,Admin")]
        public virtual async Task<IActionResult> Put(Guid id, [FromBody] TDto dto, CancellationToken cancellationToken)
            => Ok(await _service.UpdateAsync(id, dto, cancellationToken));

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Gestor,Admin")]
        public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
