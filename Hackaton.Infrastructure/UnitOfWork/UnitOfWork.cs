using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Hackaton.Domain.Interface;
using Hackaton.Domain.Interface.Markers;
using Hackaton.Infrastructure.Data;

namespace Hackaton.Infrastructure.UnitOfWork
{
    public sealed class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
    {
        private readonly AppDbContext _context;
        private readonly IUserResolver _userResolver;
        private readonly IMemoryCache _cache;
        private readonly Stack<IDbContextTransaction> _transactions = new();

        public UnitOfWork(AppDbContext context, IUserResolver userResolver, IMemoryCache cache)
        {
            _context = context;
            _userResolver = userResolver;
            _cache = cache;
        }

        public void Begin()
        {
        }

        public async Task<int> Confirm(CancellationToken cancellationToken = default)
        {
            var touchedTypes = CollectTouchedTypes();
            AplicarAuditoria();
            var result = await _context.SaveChangesAsync(cancellationToken);
            InvalidateCache(touchedTypes);
            return result;
        }

        public async Task BeginTransaction(CancellationToken cancellationToken = default)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _transactions.Push(transaction);
        }

        public async Task ConfirmTransaction(CancellationToken cancellationToken = default)
        {
            if (_transactions.Count == 0)
                throw new InvalidOperationException("Nenhuma transação ativa para confirmar.");

            var transaction = _transactions.Pop();
            try
            {
                var touchedTypes = CollectTouchedTypes();
                AplicarAuditoria();
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                InvalidateCache(touchedTypes);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public async Task RejectTransaction(CancellationToken cancellationToken = default)
        {
            if (_transactions.Count == 0) return;

            var transaction = _transactions.Pop();
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        private HashSet<string> CollectTouchedTypes()
        {
            return _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added
                         || e.State == EntityState.Modified
                         || e.State == EntityState.Deleted)
                .Select(e => e.Entity.GetType().Name)
                .ToHashSet();
        }

        private void InvalidateCache(IEnumerable<string> typeNames)
        {
            foreach (var typeName in typeNames)
                _cache.Remove(CacheKeys.All(typeName));
        }

        private void AplicarAuditoria()
        {
            var agora = DateTime.UtcNow;
            var userId = _userResolver.GetCurrentUserId();

            foreach (var entry in _context.ChangeTracker.Entries<IMustAudited>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreationTime = agora;
                        entry.Entity.CreationUserId = userId;
                        entry.Entity.ModificationTime = null;
                        entry.Entity.ModificationUserId = null;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModificationTime = agora;
                        entry.Entity.ModificationUserId = userId;
                        entry.Property(nameof(IMustAudited.CreationTime)).IsModified = false;
                        entry.Property(nameof(IMustAudited.CreationUserId)).IsModified = false;
                        break;
                }
            }
        }

        public void Dispose()
        {
            while (_transactions.Count > 0)
                _transactions.Pop().Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            while (_transactions.Count > 0)
                await _transactions.Pop().DisposeAsync();
        }
    }
}
