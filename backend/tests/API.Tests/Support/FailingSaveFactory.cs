using API.Models;
using API.Models.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Support;

internal class FailingSaveFactory(Exception exception) : PortalApiFactory
{
    private readonly Exception _exception = exception;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services => services.AddDbContext<ApplicationDbContext>(options =>
            options.AddInterceptors(new FailingSaveInterceptor(_exception))));
    }

    private class FailingSaveInterceptor(Exception exception) : SaveChangesInterceptor
    {
        private readonly Exception _exception = exception;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<Project>().Any(entry => entry.State == EntityState.Added))
            {
                throw _exception;
            }

            return ValueTask.FromResult(result);
        }
    }
}
