using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Hackaton.Domain.Entities;
using Hackaton.Domain.Interface;
using Hackaton.Infrastructure.Data;

namespace Hackaton.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly IMemoryCache _cache;

        private static readonly MemoryCacheEntryOptions CacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(30),
        };

        public GenericRepository(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _cache = cache;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var key = CacheKeys.All<T>();
            if (_cache.TryGetValue<IReadOnlyList<T>>(key, out var cached) && cached is not null)
                return cached;

            var list = await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
            _cache.Set(key, (IReadOnlyList<T>)list, CacheOptions);
            return list;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

        public virtual void Add(T entity) => _dbSet.Add(entity);

        public virtual void Update(T entity) => _dbSet.Update(entity);

        public virtual void Remove(T entity) => _dbSet.Remove(entity);
    }
}
