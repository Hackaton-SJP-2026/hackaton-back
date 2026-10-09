using System.ComponentModel;
using Hackaton.Domain.Entities;
using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hackaton.Tests.Fixtures;

// Modelos exclusivamente de teste, sem participação no domínio do produto.
public sealed class TestEntity : EntityAudited
{
    public string Text { get; set; } = string.Empty;
}

public sealed record TestDto
{
    [ReadOnly(true)]
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
}

public sealed class TestDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TestEntity>().HasKey(x => x.Id);
        builder.Entity<TestEntity>().Property(x => x.Id).ValueGeneratedNever();
    }
}
