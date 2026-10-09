using Hackaton.Domain.Entities;
using Hackaton.Domain.Interface;

namespace Hackaton.Domain.Services
{
    public class DomainService<T> : IDomainService<T> where T : Entity
    {
        protected readonly IRepository<T> _repository;

        public DomainService(IRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repository.GetAllAsync(cancellationToken);

        public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _repository.GetByIdAsync(id, cancellationToken);

        public virtual void Add(T entity) => _repository.Add(entity);

        public virtual void Update(T entity) => _repository.Update(entity);

        public virtual void Remove(T entity) => _repository.Remove(entity);
    }
}
