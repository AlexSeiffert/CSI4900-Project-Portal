using System.Diagnostics;
using System.Text.Json;
using API.Exceptions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Middleware;

public class ApiExceptionHandlerTests
{
    private readonly Mock<IProblemDetailsService> _problems = new();
    private readonly Mock<ILogger<ApiExceptionHandler>> _logger = new();

    private ApiExceptionHandler CreateHandler() => new(_problems.Object, _logger.Object);

    public static IEnumerable<object[]> DirectFailures()
    {
        yield return [new System.ComponentModel.DataAnnotations.ValidationException("Invalid request"), 400];
        yield return [new OperationCanceledException("Cancelled"), 499];
        yield return [new RepositoryException("Save", new DbUpdateConcurrencyException("Changed")), 409];
        yield return [new RepositoryException("Read", new OperationCanceledException("Cancelled")), 499];
        yield return [new RepositoryException("Read", new InvalidOperationException("Database unavailable")), 500];
    }

    [Theory]
    [MemberData(nameof(DirectFailures))]
    public async Task TryHandleAsync_ClassifiesDirectFailures(Exception exception, int status)
    {
        var context = new DefaultHttpContext();
        ProblemDetailsContext? written = null;
        _problems.Setup(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(value => written = value)
            .ReturnsAsync(true);

        Assert.True(await CreateHandler().TryHandleAsync(context, exception, CancellationToken.None));

        Assert.Equal(status, context.Response.StatusCode);
        Assert.Equal(status, written?.ProblemDetails.Status);
        if (exception is System.ComponentModel.DataAnnotations.ValidationException)
        {
            Assert.Equal(exception.Message,
                Assert.Single(Assert.IsType<ValidationProblemDetails>(written!.ProblemDetails).Errors["request"]));
        }
        if (status == 500)
        {
            Assert.DoesNotContain("Database unavailable", written!.ProblemDetails.Detail);
        }
    }

    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Cancelled, 499)]
    [InlineData(ErrorType.Unexpected, 500)]
    public async Task TryHandleAsync_WritesProblemDetailsForServiceFailure(ErrorType type, int status)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/projects";
        context.TraceIdentifier = "test-trace";
        var original = new ServiceException("GetProjects", type, "Service failed", new InvalidOperationException("Private details"));
        ProblemDetailsContext? written = null;

        _problems.Setup(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(value => written = value)
            .ReturnsAsync(true);

        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(context, original, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(status, context.Response.StatusCode);
        Assert.NotNull(written);
        Assert.Same(context, written.HttpContext);
        Assert.Equal(status, written.ProblemDetails.Status);
        Assert.Equal(context.Request.Path.Value, written.ProblemDetails.Instance);
        Assert.Equal(Activity.Current?.Id ?? context.TraceIdentifier, written.ProblemDetails.Extensions["traceId"]);
        if (type == ErrorType.Validation)
        {
            var validation = Assert.IsType<ValidationProblemDetails>(written.ProblemDetails);
            Assert.Equal(original.Message, Assert.Single(validation.Errors["request"]));
        }
        else if (type == ErrorType.NotFound)
        {
            Assert.Equal(original.Message, written.ProblemDetails.Detail);
        }
        else
        {
            Assert.DoesNotContain(original.Message, written.ProblemDetails.Detail!);
            Assert.DoesNotContain("Private details", written.ProblemDetails.Detail!);
        }
        _problems.Verify(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WritesProblemDetailsForOrchestrationFailure()
    {
        var context = new DefaultHttpContext();
        var original = new OrchestrationException("CreateProject", ErrorType.NotFound, "Supervisor not found", new KeyNotFoundException());
        ProblemDetailsContext? written = null;

        _problems.Setup(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(value => written = value)
            .ReturnsAsync(true);

        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(context, original, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.NotNull(written);
        Assert.Equal(original.Message, written.ProblemDetails.Detail);
        _problems.Verify(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_HidesUnexpectedExceptionDetails()
    {
        var context = new DefaultHttpContext();
        var original = new InvalidOperationException("Private internal details");
        ProblemDetailsContext? written = null;

        _problems.Setup(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(value => written = value)
            .ReturnsAsync(true);

        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(context, original, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.NotNull(written);
        Assert.Equal("An unexpected error occurred", written.ProblemDetails.Title);
        Assert.DoesNotContain(original.Message, written.ProblemDetails.Detail!);
        _problems.Verify(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WritesJsonWhenProblemDetailsWriterDeclines()
    {
        using var services = new ServiceCollection().AddOptions().BuildServiceProvider();
        using var body = new MemoryStream();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Path = "/api/v1/projects";
        context.Response.Body = body;
        var original = new ServiceException("CreateProject", ErrorType.Validation, "Invalid project", new ArgumentException());

        _problems.Setup(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(false);

        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(context, original, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        body.Position = 0;
        using var document = await JsonDocument.ParseAsync(body);
        Assert.Equal(400, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("/api/v1/projects", document.RootElement.GetProperty("instance").GetString());
        Assert.Equal(original.Message, document.RootElement.GetProperty("errors").GetProperty("request")[0].GetString());
        _problems.Verify(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_Returns499ForAbortedRequest()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = new DefaultHttpContext { RequestAborted = cancellation.Token };
        var original = new ServiceException("GetProjects", ErrorType.Cancelled, "Cancelled", new OperationCanceledException());
        var handler = CreateHandler();

        var handled = await handler.TryHandleAsync(context, original, cancellation.Token);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status499ClientClosedRequest, context.Response.StatusCode);
        _problems.Verify(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Never);
    }

    [Fact]
    public async Task TryHandleAsync_PropagatesProblemDetailsWriterFailure()
    {
        var context = new DefaultHttpContext();
        var original = new InvalidOperationException("Writer failed");

        _problems.Setup(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ThrowsAsync(original);

        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.TryHandleAsync(context, new Exception("Request failed"), CancellationToken.None).AsTask());

        Assert.Same(original, exception);
        _problems.Verify(problems => problems.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Once);
    }
}
