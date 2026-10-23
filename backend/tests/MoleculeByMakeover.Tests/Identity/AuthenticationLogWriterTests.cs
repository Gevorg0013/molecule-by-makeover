using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Infrastructure.Identity;
using MoleculeByMakeover.Infrastructure.Persistence;
using NSubstitute;

namespace MoleculeByMakeover.Tests.Identity;

public class AuthenticationLogWriterTests
{
    [Fact]
    public async Task Persists_the_entry_with_the_request_user_agent()
    {
        var databaseName = Guid.NewGuid().ToString(); // the options lambda runs per scope; share one store
        var services = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(databaseName))
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.UserAgent = "TestAgent/1.0 " + new string('x', 600);
        var writer = new AuthenticationLogWriter(
            services.GetRequiredService<IServiceScopeFactory>(),
            new HttpContextAccessor { HttpContext = httpContext },
            NullLogger<AuthenticationLogWriter>.Instance);

        await writer.WriteAsync(new AuthenticationLog
        {
            Email = " Jane@Example.com ",
            Succeeded = false,
            FailureReason = AuthenticationFailureReason.InvalidPassword,
            IpAddress = "203.0.113.7"
        });

        using var scope = services.CreateScope();
        var saved = Assert.Single(scope.ServiceProvider.GetRequiredService<AppDbContext>().AuthenticationLogs);
        Assert.Equal("jane@example.com", saved.Email);
        Assert.Equal(AuthenticationFailureReason.InvalidPassword, saved.FailureReason);
        Assert.StartsWith("TestAgent/1.0", saved.UserAgent);
        Assert.Equal(AuthenticationLogWriter.UserAgentMaxLength, saved.UserAgent!.Length);
        Assert.NotEqual(default, saved.OccurredAt);
    }

    [Fact]
    public async Task A_database_failure_is_swallowed_so_the_login_outcome_is_unchanged()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(_ => throw new InvalidOperationException("database is down"));
        var writer = new AuthenticationLogWriter(scopeFactory, new HttpContextAccessor(), NullLogger<AuthenticationLogWriter>.Instance);

        var exception = await Record.ExceptionAsync(() =>
            writer.WriteAsync(new AuthenticationLog { Email = "jane@example.com", Succeeded = true }));

        Assert.Null(exception);
    }
}
