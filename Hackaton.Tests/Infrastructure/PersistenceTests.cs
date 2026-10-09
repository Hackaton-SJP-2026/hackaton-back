using FluentAssertions;
using Hackaton.Domain.Interface;
using Hackaton.Infrastructure.Data;
using Hackaton.Infrastructure.Repositories;
using Hackaton.Infrastructure.UnitOfWork;
using Hackaton.Tests.Fixtures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Hackaton.Tests.Infrastructure;

public sealed class PersistenceTests
{
    [Fact]
    public async Task Confirm_PersistsAuditsAndInvalidatesListCache()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var context = new TestDbContext(options);
        await context.Database.EnsureCreatedAsync();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var user = new Mock<IUserResolver>();
        var creatorId = Guid.NewGuid();
        user.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        await using var uow = new UnitOfWork(context, user.Object, cache);
        var repository = new GenericRepository<TestEntity>(context, cache);
        (await repository.GetAllAsync()).Should().BeEmpty();
        var entity = new TestEntity { Text = "first" };
        repository.Add(entity);
        (await context.Set<TestEntity>().CountAsync()).Should().Be(0, "repository não persiste sozinho");
        await uow.Confirm();
        entity.CreationUserId.Should().Be(creatorId);
        entity.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.ModificationTime.Should().BeNull();
        (await repository.GetAllAsync()).Should().ContainSingle(x => x.Id == entity.Id);

        var creationTime = entity.CreationTime;
        var modifierId = Guid.NewGuid();
        user.Setup(x => x.GetCurrentUserId()).Returns(modifierId);
        entity.Text = "changed";
        entity.CreationUserId = Guid.NewGuid();
        entity.CreationTime = DateTime.MinValue;
        repository.Update(entity);
        await uow.Confirm();
        context.ChangeTracker.Clear();
        var persisted = await repository.GetByIdAsync(entity.Id);
        persisted!.CreationTime.Should().Be(creationTime);
        persisted.CreationUserId.Should().Be(creatorId);
        persisted.ModificationUserId.Should().Be(modifierId);
        (await repository.GetAllAsync()).Single().Text.Should().Be("changed");
    }

    [Fact]
    public async Task RejectTransaction_RollsBackDatabaseWrite()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var context = new TestDbContext(options);
        await context.Database.EnsureCreatedAsync();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        await using var uow = new UnitOfWork(context, Mock.Of<IUserResolver>(), cache);
        await uow.BeginTransaction();
        new GenericRepository<TestEntity>(context, cache).Add(new TestEntity());
        await uow.Confirm();
        await uow.RejectTransaction();
        context.ChangeTracker.Clear();
        (await context.Set<TestEntity>().CountAsync()).Should().Be(0);
    }
}
