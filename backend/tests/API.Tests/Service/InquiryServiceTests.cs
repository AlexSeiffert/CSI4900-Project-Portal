using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using API.Storage;
using Moq;

namespace API.Tests.Service;

public class InquiryServiceTests
{
    private readonly Mock<IInquiryRepository> _repository = new();
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly Mock<IAttachmentStorage> _storage = new();

    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Friend = Guid.NewGuid();
    private static readonly Guid SupervisorId = Guid.NewGuid();

    public InquiryServiceTests()
    {
        _repository.Setup(repository => repository.GetMemberEmails(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string>());
    }

    private InquiryService CreateService()
    {
        return new(_repository.Object, _projects.Object, _storage.Object);
    }

    private Inquiry Stored(InquiryStatus status, params (Guid Student, InquiryResponse Response)[] members)
    {
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = Creator, SupervisorId = SupervisorId, Status = status, Message = "Hi" };
        foreach (var (student, response) in members.Length > 0 ? members : [(Creator, InquiryResponse.Pending)])
        {
            inquiry.Members.Add(new InquiryMember { InquiryId = inquiry.Id, StudentId = student, Response = response });
        }
        _repository.Setup(repository => repository.GetInquiryById(inquiry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inquiry);
        return inquiry;
    }

    [Fact]
    public async Task GetInquiries_ReturnsMatchingInquiryDtosWithMembers()
    {
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = Creator, SupervisorId = SupervisorId, ProjectId = Guid.NewGuid(), Message = "Original message" };
        inquiry.Members.Add(new InquiryMember { StudentId = Creator });
        _repository.Setup(repository => repository.GetInquiries(CancellationToken.None)).ReturnsAsync([inquiry]);

        var result = Assert.Single(await CreateService().GetInquiries(CancellationToken.None));

        Assert.Equal(inquiry.Id, result.Id);
        Assert.Equal(inquiry.ProjectId, result.ProjectId);
        Assert.Equal(inquiry.Message, result.Message);
        Assert.Equal(Creator, Assert.Single(result.Members).StudentId);
    }

    [Fact]
    public async Task GetInquiries_PropagatesRepositoryFailure()
    {
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));
        _repository.Setup(repository => repository.GetInquiries(CancellationToken.None)).ThrowsAsync(original);

        Assert.Same(original, await Assert.ThrowsAsync<RepositoryException>(() => CreateService().GetInquiries(CancellationToken.None)));
    }

    [Fact]
    public async Task GetInquiriesByStudentId_ReturnsMatchingInquiryDtos()
    {
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = Creator };
        _repository.Setup(repository => repository.GetInquiriesByStudentId(Friend, CancellationToken.None)).ReturnsAsync([inquiry]);

        Assert.Equal(inquiry.Id, Assert.Single(await CreateService().GetInquiriesByStudentId(Friend)).Id);
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_ReturnsMatchingInquiryDtos()
    {
        var inquiry = new Inquiry { Id = Guid.NewGuid(), SupervisorId = SupervisorId };
        _repository.Setup(repository => repository.GetInquiriesBySupervisorId(SupervisorId, CancellationToken.None)).ReturnsAsync([inquiry]);

        Assert.Equal(inquiry.Id, Assert.Single(await CreateService().GetInquiriesBySupervisorId(SupervisorId)).Id);
    }

    [Fact]
    public async Task GetInquiriesByProjectId_ReturnsMatchingInquiryDtos()
    {
        var projectId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid(), ProjectId = projectId };
        _repository.Setup(repository => repository.GetInquiriesByProjectId(projectId, CancellationToken.None)).ReturnsAsync([inquiry]);

        Assert.Equal(inquiry.Id, Assert.Single(await CreateService().GetInquiriesByProjectId(projectId)).Id);
    }

    [Fact]
    public async Task GetInquiryById_FillsMemberEmailsForAMember()
    {
        var inquiry = Stored(InquiryStatus.Draft, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));
        _repository.Setup(repository => repository.GetMemberEmails(inquiry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [Creator] = "c@x.test", [Friend] = "f@x.test" });

        var result = await CreateService().GetInquiryById(inquiry.Id, new Caller(Friend));

        Assert.Equal(["c@x.test", "f@x.test"], result.Members.Select(member => member.Email));
    }

    [Fact]
    public async Task GetInquiryById_HidesTheInquiryFromOutsiders()
    {
        var inquiry = Stored(InquiryStatus.Sent);

        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            CreateService().GetInquiryById(inquiry.Id, new Caller(Guid.NewGuid(), false)));

        Assert.Equal(ErrorType.NotFound, exception.Type);
    }

    [Fact]
    public async Task GetInquiryById_PropagatesRepositoryFailure()
    {
        var inquiryId = Guid.NewGuid();
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));
        _repository.Setup(repository => repository.GetInquiryById(inquiryId, It.IsAny<CancellationToken>())).ThrowsAsync(original);

        Assert.Same(original, await Assert.ThrowsAsync<RepositoryException>(() => CreateService().GetInquiryById(inquiryId, new Caller(Creator))));
    }

    [Fact]
    public async Task CreateInquiry_StartsAsDraftWithTheProjectsSupervisorAndTheCreatorAsMember()
    {
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = SupervisorId };
        _projects.Setup(projects => projects.GetProjectById(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        Inquiry? saved = null;
        _repository.Setup(repository => repository.CreateInquiry(It.IsAny<Inquiry>(), It.IsAny<CancellationToken>()))
            .Callback((Inquiry value, CancellationToken _) => saved = value)
            .ReturnsAsync((Inquiry value, CancellationToken _) => value);

        var result = await CreateService().CreateInquiry(new CreateInquiry { StudentId = Creator, ProjectId = project.Id, Message = "  Interested  " });

        Assert.Equal(InquiryStatus.Draft, result.Status);
        Assert.Equal(SupervisorId, result.SupervisorId);
        Assert.Equal("Interested", result.Message);
        Assert.Equal(Creator, Assert.Single(saved!.Members).StudentId);
    }

    [Fact]
    public async Task CreateInquiry_RejectsAProjectThatDoesNotExist()
    {
        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            CreateService().CreateInquiry(new CreateInquiry { StudentId = Creator, ProjectId = Guid.NewGuid() }));

        Assert.Equal(ErrorType.Validation, exception.Type);
        _repository.Verify(repository => repository.CreateInquiry(It.IsAny<Inquiry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateInquiry_RejectsAMessageOverTheLimit()
    {
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = SupervisorId };
        _projects.Setup(projects => projects.GetProjectById(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().CreateInquiry(
            new CreateInquiry { StudentId = Creator, ProjectId = project.Id, Message = new string('x', CreateInquiry.MessageMaxLength + 1) }));

        Assert.Equal(ErrorType.Validation, exception.Type);
    }

    [Fact]
    public async Task AddMember_AddsARegisteredStudentByEmail()
    {
        var inquiry = Stored(InquiryStatus.Draft);
        _repository.Setup(repository => repository.FindStudentIdByEmail("friend@x.test", It.IsAny<CancellationToken>())).ReturnsAsync(Friend);

        await CreateService().AddMember(inquiry.Id, new AddInquiryMember { Email = "friend@x.test" }, new Caller(Creator));

        _repository.Verify(repository => repository.AddMember(
            It.Is<InquiryMember>(member => member.InquiryId == inquiry.Id && member.StudentId == Friend && member.Response == InquiryResponse.Pending),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMember_RejectsAnUnregisteredEmail()
    {
        var inquiry = Stored(InquiryStatus.Draft);

        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            CreateService().AddMember(inquiry.Id, new AddInquiryMember { Email = "nobody@x.test" }, new Caller(Creator)));

        Assert.Equal(ErrorType.Validation, exception.Type);
        Assert.Contains("No student", exception.Message);
    }

    [Fact]
    public async Task AddMember_RejectsAFourthMember()
    {
        var inquiry = Stored(InquiryStatus.Draft, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending), (Guid.NewGuid(), InquiryResponse.Pending));

        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            CreateService().AddMember(inquiry.Id, new AddInquiryMember { Email = "fourth@x.test" }, new Caller(Creator)));

        Assert.Equal(ErrorType.Validation, exception.Type);
        _repository.Verify(repository => repository.FindStudentIdByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddMember_RejectsSomeoneAlreadyOnTheRequest()
    {
        var inquiry = Stored(InquiryStatus.Draft, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));
        _repository.Setup(repository => repository.FindStudentIdByEmail("friend@x.test", It.IsAny<CancellationToken>())).ReturnsAsync(Friend);

        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            CreateService().AddMember(inquiry.Id, new AddInquiryMember { Email = "friend@x.test" }, new Caller(Creator)));

        Assert.Equal(ErrorType.Validation, exception.Type);
    }

    [Fact]
    public async Task AddMember_OnlyTheCreatorCanAdd()
    {
        var inquiry = Stored(InquiryStatus.Draft, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));

        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            CreateService().AddMember(inquiry.Id, new AddInquiryMember { Email = "x@x.test" }, new Caller(Friend)));

        Assert.Equal(ErrorType.Forbidden, exception.Type);
    }

    [Fact]
    public async Task AddMember_OnlyWhileDraft()
    {
        var inquiry = Stored(InquiryStatus.Sent);

        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            CreateService().AddMember(inquiry.Id, new AddInquiryMember { Email = "x@x.test" }, new Caller(Creator)));

        Assert.Equal(ErrorType.Validation, exception.Type);
    }

    [Fact]
    public async Task RemoveMember_CreatorRemovesAFriendFromADraft()
    {
        var inquiry = Stored(InquiryStatus.Draft, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));

        await CreateService().RemoveMember(inquiry.Id, Friend, new Caller(Creator));

        _repository.Verify(repository => repository.RemoveMember(inquiry.Id, Friend, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveMember_TheCreatorCannotBeRemoved()
    {
        var inquiry = Stored(InquiryStatus.Draft, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().RemoveMember(inquiry.Id, Creator, new Caller(Creator)));

        Assert.Equal(ErrorType.Validation, exception.Type);
    }

    [Fact]
    public async Task MarkSent_AMemberMovesADraftToSent()
    {
        var inquiry = Stored(InquiryStatus.Draft, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));

        var result = await CreateService().MarkSent(inquiry.Id, new Caller(Friend));

        Assert.Equal(InquiryStatus.Sent, result.Status);
        _repository.Verify(repository => repository.SaveInquiry(inquiry, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkSent_TheSupervisorCannot()
    {
        var inquiry = Stored(InquiryStatus.Draft);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().MarkSent(inquiry.Id, new Caller(SupervisorId)));

        Assert.Equal(ErrorType.Forbidden, exception.Type);
    }

    [Fact]
    public async Task Offer_TheSupervisorOffersASentRequest()
    {
        var inquiry = Stored(InquiryStatus.Sent);

        var result = await CreateService().Offer(inquiry.Id, new Caller(SupervisorId));

        Assert.Equal(InquiryStatus.Offered, result.Status);
    }

    [Theory]
    [InlineData(InquiryStatus.Draft)]
    [InlineData(InquiryStatus.Matched)]
    [InlineData(InquiryStatus.Closed)]
    public async Task Offer_OnlyFromSent(InquiryStatus status)
    {
        var inquiry = Stored(status);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().Offer(inquiry.Id, new Caller(SupervisorId)));

        Assert.Equal(ErrorType.Validation, exception.Type);
        _repository.Verify(repository => repository.SaveInquiry(It.IsAny<Inquiry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Offer_AStudentCannot()
    {
        var inquiry = Stored(InquiryStatus.Sent);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().Offer(inquiry.Id, new Caller(Creator)));

        Assert.Equal(ErrorType.Forbidden, exception.Type);
    }

    [Theory]
    [InlineData(InquiryStatus.Draft)]
    [InlineData(InquiryStatus.Sent)]
    [InlineData(InquiryStatus.Offered)]
    [InlineData(InquiryStatus.Matched)]
    public async Task Close_TheSupervisorCanCloseAtAnyTime(InquiryStatus status)
    {
        var inquiry = Stored(status);

        var result = await CreateService().Close(inquiry.Id, new Caller(SupervisorId));

        Assert.Equal(InquiryStatus.Closed, result.Status);
    }

    [Fact]
    public async Task Close_AStudentCannot()
    {
        var inquiry = Stored(InquiryStatus.Sent);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().Close(inquiry.Id, new Caller(Creator)));

        Assert.Equal(ErrorType.Forbidden, exception.Type);
    }

    [Fact]
    public async Task Respond_PassesTheAnswerToTheRepositoryAndReturnsTheDroppedOffers()
    {
        var inquiry = Stored(InquiryStatus.Offered, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));
        var dropped = Guid.NewGuid();
        _repository.Setup(repository => repository.Respond(inquiry.Id, Friend, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RespondOutcome(InquiryStatus.Offered, [dropped]));

        var result = await CreateService().Respond(inquiry.Id, true, new Caller(Friend));

        Assert.Equal(InquiryStatus.Offered, result.Status);
        Assert.Equal([dropped], result.DroppedInquiryIds);
    }

    [Fact]
    public async Task Respond_TheSupervisorCannotAnswer()
    {
        var inquiry = Stored(InquiryStatus.Offered);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().Respond(inquiry.Id, true, new Caller(SupervisorId)));

        Assert.Equal(ErrorType.Forbidden, exception.Type);
        _repository.Verify(repository => repository.Respond(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Respond_OnlyOnAnOpenOffer()
    {
        var inquiry = Stored(InquiryStatus.Sent);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().Respond(inquiry.Id, true, new Caller(Creator)));

        Assert.Equal(ErrorType.Validation, exception.Type);
    }

    [Fact]
    public async Task Respond_OnlyOnce()
    {
        var inquiry = Stored(InquiryStatus.Offered, (Creator, InquiryResponse.Accepted), (Friend, InquiryResponse.Pending));

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().Respond(inquiry.Id, false, new Caller(Creator)));

        Assert.Equal(ErrorType.Validation, exception.Type);
    }

    [Fact]
    public async Task CreateUploadUrl_SignsAPathInTheMembersOwnFolder()
    {
        var inquiry = Stored(InquiryStatus.Draft, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));
        _storage.Setup(storage => storage.CreateUploadUrl(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string path, CancellationToken _) => "https://storage/" + path);

        var result = await CreateService().CreateUploadUrl(inquiry.Id, new CreateUploadUrl { Kind = AttachmentKind.Transcript }, new Caller(Friend));

        Assert.StartsWith($"{Friend}/{inquiry.Id}/transcript-", result.StoragePath);
        Assert.EndsWith(".pdf", result.StoragePath);
        Assert.Equal("https://storage/" + result.StoragePath, result.Url);
    }

    [Fact]
    public async Task CreateUploadUrl_ANonMemberCannot()
    {
        var inquiry = Stored(InquiryStatus.Draft);

        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            CreateService().CreateUploadUrl(inquiry.Id, new CreateUploadUrl { Kind = AttachmentKind.Resume }, new Caller(SupervisorId)));

        Assert.Equal(ErrorType.Forbidden, exception.Type);
    }

    [Fact]
    public async Task RegisterAttachment_SavesTheFileForTheUploader()
    {
        var inquiry = Stored(InquiryStatus.Sent, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));
        _repository.Setup(repository => repository.AddAttachment(It.IsAny<InquiryAttachment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InquiryAttachment value, CancellationToken _) => value);

        var result = await CreateService().RegisterAttachment(inquiry.Id,
            new RegisterAttachment { StoragePath = $"{Friend}/{inquiry.Id}/resume-1.pdf", Kind = AttachmentKind.Resume, SizeBytes = 100 }, new Caller(Friend));

        Assert.Equal(Friend, result.StudentId);
        Assert.Equal(AttachmentKind.Resume, result.Kind);
    }

    [Fact]
    public async Task RegisterAttachment_RejectsAPathInAnotherMembersFolder()
    {
        var inquiry = Stored(InquiryStatus.Sent, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().RegisterAttachment(inquiry.Id,
            new RegisterAttachment { StoragePath = $"{Creator}/{inquiry.Id}/resume-1.pdf", Kind = AttachmentKind.Resume, SizeBytes = 100 }, new Caller(Friend)));

        Assert.Equal(ErrorType.Validation, exception.Type);
    }

    [Fact]
    public async Task CreateDownloadUrl_TheSupervisorCanViewAMembersFile()
    {
        var inquiry = Stored(InquiryStatus.Sent, (Creator, InquiryResponse.Pending), (Friend, InquiryResponse.Pending));
        var attachment = new InquiryAttachment { Id = Guid.NewGuid(), InquiryId = inquiry.Id, StudentId = Friend, StoragePath = "f/i/x.pdf" };
        inquiry.Attachments.Add(attachment);
        _storage.Setup(storage => storage.CreateDownloadUrl("f/i/x.pdf", It.IsAny<CancellationToken>())).ReturnsAsync("https://signed");

        var result = await CreateService().CreateDownloadUrl(inquiry.Id, attachment.Id, new Caller(SupervisorId));

        Assert.Equal("https://signed", result.Url);
    }

    [Fact]
    public async Task CreateDownloadUrl_AnotherSupervisorIsDenied()
    {
        var inquiry = Stored(InquiryStatus.Sent);
        var attachment = new InquiryAttachment { Id = Guid.NewGuid(), InquiryId = inquiry.Id, StudentId = Creator, StoragePath = "c/i/x.pdf" };
        inquiry.Attachments.Add(attachment);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => CreateService().CreateDownloadUrl(inquiry.Id, attachment.Id, new Caller(Guid.NewGuid())));

        Assert.Equal(ErrorType.NotFound, exception.Type);
        _storage.Verify(storage => storage.CreateDownloadUrl(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
