using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MudBlazor;
using uoProjects.Services;

namespace uoProjects.Tests.Support;

public abstract class ServiceTestBase : IDisposable
{
    protected Mock<ISnackbar> Snackbar { get; } = new();
    protected HttpClient Http { get; }
    protected NotificationService Notifications { get; }

    private Func<HttpResponseMessage> respond = () => throw new InvalidOperationException("Configure a response before fetching.");

    protected ServiceTestBase()
    {
        Http = new HttpClient(new StubHandler(() => respond())) { BaseAddress = new Uri("https://api.example.test/") };
        Notifications = new NotificationService(Snackbar.Object, NullLogger<NotificationService>.Instance);
    }

    protected void RespondWith<T>(T data)
        => respond = () => new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) };

    protected void RespondWith(HttpStatusCode status)
        => respond = () => new HttpResponseMessage(status);

    protected void ThrowOnRequest(Exception exception)
        => respond = () => throw exception;

    protected void VerifyErrorNotification(string message)
        => Snackbar.Verify(service => service.Add(
            It.Is<string>(text => text.Contains(message)), Severity.Error,
            It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);

    public void Dispose() => Http.Dispose();

    private sealed class StubHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond());
    }
}
