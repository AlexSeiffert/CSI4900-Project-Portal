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
        Assert.Null(result[0].Student);
        Assert.Null(result[0].Supervisor);
        Assert.Null(result[0].Project);
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
        Assert.Equal(context.Student.UserId, result.Student.UserId);
        Assert.Equal(context.Supervisor.UserId, result.Supervisor.UserId);
        Assert.Equal(context.Project.Id, result.Project?.Id);
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
        Assert.Equal(context.Student.UserId, inquiry.Student.UserId);
        Assert.Equal(context.Supervisor.UserId, inquiry.Supervisor.UserId);
        Assert.Null(inquiry.Project);
        context.Db.ChangeTracker.Clear();
        var persisted = await context.Db.Inquiries.SingleAsync(x => x.Id == inquiry.Id);
        Assert.Null(persisted.ProjectId);
        Assert.Null(persisted.Message);
        Assert.Equal(InquiryStatus.Draft, persisted.Status);
        Assert.NotEqual(default, persisted.CreatedAt);
        Assert.Equal(persisted.CreatedAt, persisted.UpdatedAt);
        var withRelations = await repository.GetInquiryById(inquiry.Id);
        Assert.Equal(context.Student.UserId, withRelations?.Student.UserId);
        Assert.Equal(context.Supervisor.UserId, withRelations?.Supervisor.UserId);
        Assert.Null(withRelations?.Project);
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

    private static async Task<Inquiry> AddOffer(RepositoryTestContext context, params (Guid Student, InquiryResponse Response)[] members)
    {
        var inquiry = new Inquiry
        {
            Id = Guid.NewGuid(), StudentId = members[0].Student, SupervisorId = context.Supervisor.UserId,
            Status = InquiryStatus.Offered, CreatedAt = DateTime.UtcNow
        };
        foreach (var (student, response) in members)
        {
            inquiry.Members.Add(new InquiryMember { StudentId = student, Response = response });
        }
        context.Db.Inquiries.Add(inquiry);
        await context.Db.SaveChangesAsync();
        context.Db.ChangeTracker.Clear();
        return inquiry;
    }

    private static async Task<Student> AddStudent(RepositoryTestContext context, string name)
    {
        var student = new Student { UserId = Guid.NewGuid(), DisplayName = name };
        context.Db.Students.Add(student);
        await context.Db.SaveChangesAsync();
        context.Db.ChangeTracker.Clear();
        return student;
    }

    [Fact]
    public async Task GetInquiriesByStudentId_IncludesInquiriesTheStudentWasAddedTo()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);
        await repository.AddMember(new InquiryMember { InquiryId = context.OtherInquiry.Id, StudentId = context.Student.UserId });

        var result = await repository.GetInquiriesByStudentId(context.Student.UserId);

        Assert.Equal(new[] { context.Inquiry.Id, context.OtherInquiry.Id }, result.Select(x => x.Id));
        Assert.Equal(2, result[1].Members.Count);
    }

    [Fact]
    public async Task FindStudentIdByEmail_IgnoresCaseAndSpaces()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        Assert.Equal(context.OtherStudent.UserId, await repository.FindStudentIdByEmail("  ALEX@student.test "));
    }

    [Fact]
    public async Task FindStudentIdByEmail_ReturnsNullForUnknownOrNonStudentEmails()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        Assert.Null(await repository.FindStudentIdByEmail("nobody@student.test"));
        Assert.Null(await repository.FindStudentIdByEmail(context.Supervisor.ContactEmail));
    }

    [Fact]
    public async Task GetMemberEmails_ReturnsEachMembersEmail()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);
        await repository.AddMember(new InquiryMember { InquiryId = context.Inquiry.Id, StudentId = context.OtherStudent.UserId });

        var result = await repository.GetMemberEmails(context.Inquiry.Id);

        Assert.Equal("zoe@student.test", result[context.Student.UserId]);
        Assert.Equal("alex@student.test", result[context.OtherStudent.UserId]);
    }

    [Fact]
    public async Task AddMember_RejectsTheSameStudentTwice()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);

        await Assert.ThrowsAsync<RepositoryException>(() =>
            repository.AddMember(new InquiryMember { InquiryId = context.Inquiry.Id, StudentId = context.Student.UserId }));
    }

    [Fact]
    public async Task RemoveMember_AlsoRemovesTheirAttachments()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);
        await repository.AddMember(new InquiryMember { InquiryId = context.Inquiry.Id, StudentId = context.OtherStudent.UserId });
        await repository.AddAttachment(new InquiryAttachment { Id = Guid.NewGuid(), InquiryId = context.Inquiry.Id, StudentId = context.OtherStudent.UserId, Kind = AttachmentKind.Resume, StoragePath = "a/b/c.pdf", MimeType = "application/pdf", SizeBytes = 10 });
        await repository.AddAttachment(new InquiryAttachment { Id = Guid.NewGuid(), InquiryId = context.Inquiry.Id, StudentId = context.Student.UserId, Kind = AttachmentKind.Transcript, StoragePath = "d/b/c.pdf", MimeType = "application/pdf", SizeBytes = 10 });
        context.Db.ChangeTracker.Clear();

        Assert.True(await repository.RemoveMember(context.Inquiry.Id, context.OtherStudent.UserId));
        Assert.False(await repository.RemoveMember(context.Inquiry.Id, context.OtherStudent.UserId));

        Assert.Equal(context.Student.UserId, Assert.Single(await context.Db.InquiryAttachments.ToListAsync()).StudentId);
        Assert.Single(await context.Db.InquiryMembers.Where(x => x.InquiryId == context.Inquiry.Id).ToListAsync());
    }

    [Fact]
    public async Task SaveInquiry_PersistsTheNewStatus()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new InquiryRepository(context.Db);
        var inquiry = (await repository.GetInquiryById(context.Inquiry.Id))!;

        inquiry.Status = InquiryStatus.Sent;
        await repository.SaveInquiry(inquiry);
        context.Db.ChangeTracker.Clear();

        Assert.Equal(InquiryStatus.Sent, (await context.Db.Inquiries.SingleAsync(x => x.Id == context.Inquiry.Id)).Status);
    }

    [Fact]
    public async Task Respond_StaysOfferedWhileOthersArePending()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var offer = await AddOffer(context, (context.Student.UserId, InquiryResponse.Pending), (context.OtherStudent.UserId, InquiryResponse.Pending));

        var result = await new InquiryRepository(context.Db).Respond(offer.Id, context.Student.UserId, true);

        Assert.Equal(InquiryStatus.Offered, result.Status);
        Assert.Empty(result.DroppedInquiryIds);
    }

    [Fact]
    public async Task Respond_TwoAcceptAndOneDeclines_IsMatchedWithoutTheDecliner()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var third = await AddStudent(context, "Third");
        var offer = await AddOffer(context, (context.Student.UserId, InquiryResponse.Pending),
            (context.OtherStudent.UserId, InquiryResponse.Pending), (third.UserId, InquiryResponse.Pending));
        var repository = new InquiryRepository(context.Db);

        await repository.Respond(offer.Id, third.UserId, false);
        await repository.Respond(offer.Id, context.Student.UserId, true);
        var result = await repository.Respond(offer.Id, context.OtherStudent.UserId, true);

        Assert.Equal(InquiryStatus.Matched, result.Status);
        context.Db.ChangeTracker.Clear();
        var members = await context.Db.InquiryMembers.Where(x => x.InquiryId == offer.Id).ToListAsync();
        Assert.Equal(2, members.Count);
        Assert.All(members, member => Assert.Equal(InquiryResponse.Accepted, member.Response));
        Assert.DoesNotContain(members, member => member.StudentId == third.UserId);
    }

    [Fact]
    public async Task Respond_EveryoneDeclines_Closes()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var offer = await AddOffer(context, (context.Student.UserId, InquiryResponse.Pending), (context.OtherStudent.UserId, InquiryResponse.Pending));
        var repository = new InquiryRepository(context.Db);

        await repository.Respond(offer.Id, context.Student.UserId, false);
        var result = await repository.Respond(offer.Id, context.OtherStudent.UserId, false);

        Assert.Equal(InquiryStatus.Closed, result.Status);
        context.Db.ChangeTracker.Clear();
        Assert.Empty(await context.Db.InquiryMembers.Where(x => x.InquiryId == offer.Id).ToListAsync());
    }

    [Fact]
    public async Task Respond_AcceptingDropsTheStudentFromTheirOtherOffers()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var chosen = await AddOffer(context, (context.Student.UserId, InquiryResponse.Pending));
        var other = await AddOffer(context, (context.OtherStudent.UserId, InquiryResponse.Accepted), (context.Student.UserId, InquiryResponse.Pending));
        var alone = await AddOffer(context, (context.Student.UserId, InquiryResponse.Pending));

        var result = await new InquiryRepository(context.Db).Respond(chosen.Id, context.Student.UserId, true);

        Assert.Equal(InquiryStatus.Matched, result.Status);
        Assert.Equal(new[] { other.Id, alone.Id }.Order(), result.DroppedInquiryIds.Order());
        context.Db.ChangeTracker.Clear();
        Assert.Equal(InquiryStatus.Matched, (await context.Db.Inquiries.SingleAsync(x => x.Id == other.Id)).Status);
        Assert.Equal(InquiryStatus.Closed, (await context.Db.Inquiries.SingleAsync(x => x.Id == alone.Id)).Status);
        Assert.False(await context.Db.InquiryMembers.AnyAsync(x => x.StudentId == context.Student.UserId && (x.InquiryId == other.Id || x.InquiryId == alone.Id)));
        Assert.Equal(InquiryStatus.Draft, (await context.Db.Inquiries.SingleAsync(x => x.Id == context.Inquiry.Id)).Status);
        Assert.True(await context.Db.InquiryMembers.AnyAsync(x => x.StudentId == context.Student.UserId && x.InquiryId == context.Inquiry.Id));
    }

    [Fact]
    public async Task Respond_RejectsAnOfferThatIsNoLongerOpen()
    {
        await using var context = await RepositoryTestContext.CreateAsync();

        var exception = await Assert.ThrowsAsync<RepositoryException>(() =>
            new InquiryRepository(context.Db).Respond(context.Inquiry.Id, context.Student.UserId, true));

        Assert.Equal(ErrorType.Conflict, exception.Type);
    }
}
