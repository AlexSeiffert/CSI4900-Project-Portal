using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Repository;

public class InquiryRepositoryTests
{
    [Fact]
    public async Task GetInquiries_ReturnsNewestFirstWithoutTracking()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        var result = await repository.GetInquiries();

        Assert.Equal(new[] { context.Inquiry.Id, context.OtherInquiry.Id }, result.Select(x => x.Id));
        Assert.Empty(context.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetInquiries_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Inquiry>())
            .Throws(original);

        var repository = new InquiryRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiries(CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Inquiry>(), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesByStudentId_ReturnsOnlyMatches()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        var result = Assert.Single(await repository.GetInquiriesByStudentId(context.Student.UserId));

        Assert.Equal(context.Inquiry.Id, result.Id);
    }

    [Fact]
    public async Task GetInquiriesByStudentId_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Inquiry>())
            .Throws(original);

        var repository = new InquiryRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiriesByStudentId(context.Student.UserId, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Inquiry>(), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_ReturnsOnlyMatches()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        var result = Assert.Single(await repository.GetInquiriesBySupervisorId(context.Supervisor.UserId));

        Assert.Equal(context.Inquiry.Id, result.Id);
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Inquiry>())
            .Throws(original);

        var repository = new InquiryRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiriesBySupervisorId(context.Supervisor.UserId, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Inquiry>(), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesByProjectId_ReturnsOnlyMatches()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        var result = Assert.Single(await repository.GetInquiriesByProjectId(context.Project.Id));

        Assert.Equal(context.Inquiry.Id, result.Id);
    }

    [Fact]
    public async Task GetInquiriesByProjectId_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Inquiry>())
            .Throws(original);

        var repository = new InquiryRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiriesByProjectId(context.Project.Id, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Inquiry>(), Times.Once);
    }

    [Fact]
    public async Task GetInquiryById_ReturnsTrackedInquiry()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        var result = await repository.GetInquiryById(context.Inquiry.Id);

        Assert.NotNull(result);
        Assert.Equal(context.Inquiry.Id, result.Id);
        Assert.Equal(EntityState.Unchanged, context.Db.Entry(result).State);
    }

    [Fact]
    public async Task GetInquiryById_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Inquiry>())
            .Throws(original);

        var repository = new InquiryRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiryById(context.Inquiry.Id, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Inquiry>(), Times.Once);
    }

    [Fact]
    public async Task CreateInquiry_PersistsGeneralInquiryWithNullOptionalFields()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = context.Student.UserId, SupervisorId = context.Supervisor.UserId };

        Assert.Same(inquiry, await repository.CreateInquiry(inquiry));
        context.Db.ChangeTracker.Clear();
        var persisted = await context.Db.Inquiries.SingleAsync(x => x.Id == inquiry.Id);
        Assert.Null(persisted.ProjectId);
        Assert.Null(persisted.Message);
        Assert.Equal(InquiryStatus.Pending, persisted.Status);
    }

    [Fact]
    public async Task CreateInquiry_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Inquiry>())
            .Throws(original);

        var repository = new InquiryRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateInquiry(new Inquiry { Id = Guid.NewGuid(), StudentId = context.Student.UserId, SupervisorId = context.Supervisor.UserId }, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Inquiry>(), Times.Once);
    }

    [Fact]
    public async Task CreateInquiry_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateInquiry(new Inquiry { Id = Guid.NewGuid(), StudentId = context.Student.UserId, SupervisorId = context.Supervisor.UserId }));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateInquiry_PersistsStatusAndPreservesMessage()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);
        var inquiry = (await repository.GetInquiryById(context.Inquiry.Id))!;
        inquiry.Status = InquiryStatus.Responded;

        Assert.Same(inquiry, await repository.UpdateInquiry(inquiry));
        context.Db.ChangeTracker.Clear();
        var persisted = (await repository.GetInquiryById(inquiry.Id))!;
        Assert.Equal(InquiryStatus.Responded, persisted.Status);
        Assert.Equal("Original", persisted.Message);
        Assert.Equal(context.Project.Id, persisted.ProjectId);
    }

    [Fact]
    public async Task UpdateInquiry_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);
        var entity = (await repository.GetInquiryById(context.Inquiry.Id))!;
        entity.Status = InquiryStatus.Responded;
        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateInquiry(entity));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
