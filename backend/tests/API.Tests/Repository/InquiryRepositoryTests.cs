using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Repository;

public class InquiryRepositoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetInquiries_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiries(token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetInquiriesByStudentId_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiriesByStudentId(c.Student.UserId, token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetInquiriesBySupervisorId_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiriesBySupervisorId(c.Supervisor.UserId, token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetInquiriesByProjectId_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiriesByProjectId(c.Project.Id, token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetInquiryById_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetInquiryById(c.Inquiry.Id, token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateInquiry(new Inquiry { Id = Guid.NewGuid(), StudentId = c.Student.UserId, SupervisorId = c.Supervisor.UserId }, token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateInquiry_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var inquiry = (await repository.GetInquiryById(c.Inquiry.Id))!;
        inquiry.Status = InquiryStatus.Responded;
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateInquiry(inquiry, token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Fact]
    public async Task GetInquiries_ReturnsNewestFirstWithoutTracking()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var result = await repository.GetInquiries();
        Assert.Equal(new[] { c.Inquiry.Id, c.OtherInquiry.Id }, result.Select(x => x.Id));
        Assert.Empty(c.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetInquiriesByStudentId_ReturnsOnlyMatches()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var result = Assert.Single(await repository.GetInquiriesByStudentId(c.Student.UserId));
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Empty(await repository.GetInquiriesByStudentId(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_ReturnsOnlyMatches()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var result = Assert.Single(await repository.GetInquiriesBySupervisorId(c.Supervisor.UserId));
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Empty(await repository.GetInquiriesBySupervisorId(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetInquiriesByProjectId_ReturnsOnlyMatches()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var result = Assert.Single(await repository.GetInquiriesByProjectId(c.Project.Id));
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Empty(await repository.GetInquiriesByProjectId(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetInquiryById_ReturnsTrackedInquiryOrNull()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var result = await repository.GetInquiryById(c.Inquiry.Id);
        Assert.NotNull(result);
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Equal(EntityState.Unchanged, c.Db.Entry(result).State);
        Assert.Null(await repository.GetInquiryById(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateInquiry_PersistsGeneralInquiryWithNullOptionalFields()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = c.Student.UserId, SupervisorId = c.Supervisor.UserId };
        Assert.Same(inquiry, await repository.CreateInquiry(inquiry));
        c.Db.ChangeTracker.Clear();
        var persisted = await c.Db.Inquiries.SingleAsync(x => x.Id == inquiry.Id);
        Assert.Null(persisted.ProjectId);
        Assert.Null(persisted.Message);
        Assert.Equal(InquiryStatus.Pending, persisted.Status);
    }

    [Fact]
    public async Task UpdateInquiry_PersistsStatusAndPreservesMessage()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var inquiry = (await repository.GetInquiryById(c.Inquiry.Id))!;
        inquiry.Status = InquiryStatus.Responded;
        Assert.Same(inquiry, await repository.UpdateInquiry(inquiry));
        c.Db.ChangeTracker.Clear();
        var persisted = (await repository.GetInquiryById(inquiry.Id))!;
        Assert.Equal(InquiryStatus.Responded, persisted.Status);
        Assert.Equal("Original", persisted.Message);
        Assert.Equal(c.Project.Id, persisted.ProjectId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);

        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateInquiry(new Inquiry { Id = Guid.NewGuid(), StudentId = c.Student.UserId, SupervisorId = c.Supervisor.UserId }));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorKind.Conflict : ErrorKind.Unexpected, exception.Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateInquiry_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(c.Db);
        var entity = (await repository.GetInquiryById(c.Inquiry.Id))!;
entity.Status = InquiryStatus.Responded;
        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateInquiry(entity));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorKind.Conflict : ErrorKind.Unexpected, exception.Kind);
    }
}
