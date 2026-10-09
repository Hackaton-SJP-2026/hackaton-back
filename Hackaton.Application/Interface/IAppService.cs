using Hackaton.Domain.Entities;

namespace Hackaton.Application.Interface
{
    public interface IAppService<TEntity, TDto> where TEntity : Entity
    {
        Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TDto> CreateAsync(TDto dto, CancellationToken cancellationToken = default);
        Task<TDto> UpdateAsync(Guid id, TDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
