using uoProjects.Exceptions;
using uoProjects.Tests.Support;

namespace uoProjects.Tests.Services;

public class NotificationServiceTests : ServiceTestBase
{
    [Fact]
    public void Error_ShowsFailureNotification()
    {
        var exception = new ServiceException("GetProjectsAsync", "The request timed out.");

        Notifications.Error("An error occurred when fetching projects.", exception);

        VerifyErrorNotification("An error occurred when fetching projects. The request timed out.");
    }
}
