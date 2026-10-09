using FluentAssertions;
using FluentValidation;
using Hackaton.Application.Exceptions;
using Hackaton.Application.Services;
using Hackaton.Domain.Interface;
using Hackaton.Tests.Fixtures;
using Mapster;
using MapsterMapper;
using Moq;
using Xunit;

namespace Hackaton.Tests.Application;

public sealed class AppServiceTests
{
    private static AppService<TestEntity, TestDto> Create(
        Mock<IDomainService<TestEntity>> domain, Mock<IUnitOfWork> uow,
        params IValidator<TestDto>[] validators)
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<TestEntity, TestDto>();
        config.NewConfig<TestDto, TestEntity>().Ignore(x => x.Id);
        return new(domain.Object, uow.Object, new Mapper(config), validators);
    }

    [Fact]
    public async Task Create_GeneratesIdAndConfirmsUnitOfWork()
    {
        var domain = new Mock<IDomainService<TestEntity>>();
        var uow = new Mock<IUnitOfWork>();
        var clientId = Guid.NewGuid();
        var result = await Create(domain, uow).CreateAsync(new TestDto { Id = clientId, Text = "example" });
        result.Id.Should().NotBeEmpty().And.NotBe(clientId);
        result.Text.Should().Be("example");
        domain.Verify(x => x.Add(It.Is<TestEntity>(e => e.Id == result.Id)), Times.Once);
        uow.Verify(x => x.Begin(), Times.Once);
        uow.Verify(x => x.Confirm(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidDto_DoesNotWrite()
    {
        var domain = new Mock<IDomainService<TestEntity>>();
        var uow = new Mock<IUnitOfWork>();
        var validator = new InlineValidator<TestDto>();
        validator.RuleFor(x => x.Text).NotEmpty();
        var act = () => Create(domain, uow, validator).CreateAsync(new TestDto());
        await act.Should().ThrowAsync<EntityValidationException>();
        domain.Verify(x => x.Add(It.IsAny<TestEntity>()), Times.Never);
        uow.Verify(x => x.Confirm(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_UsesUrlIdAndExistingEntity()
    {
        var id = Guid.NewGuid();
        var existing = new TestEntity { Id = id, Text = "before" };
        var domain = new Mock<IDomainService<TestEntity>>();
        domain.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var uow = new Mock<IUnitOfWork>();
        var result = await Create(domain, uow).UpdateAsync(id, new TestDto { Id = Guid.NewGuid(), Text = "after" });
        result.Id.Should().Be(id);
        existing.Text.Should().Be("after");
        domain.Verify(x => x.Update(existing), Times.Once);
        uow.Verify(x => x.Confirm(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MissingEntity_DoesNotConfirmDeletion()
    {
        var domain = new Mock<IDomainService<TestEntity>>();
        var uow = new Mock<IUnitOfWork>();
        var act = () => Create(domain, uow).DeleteAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<EntityNotFoundException>();
        uow.Verify(x => x.Confirm(It.IsAny<CancellationToken>()), Times.Never);
    }
}
