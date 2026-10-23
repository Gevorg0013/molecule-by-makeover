using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Application.Common.Options;
using MoleculeByMakeover.Application.Features.Identity;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;
using NSubstitute;

namespace MoleculeByMakeover.Tests.Identity;

public class AuthServiceLoginTests
{
    private const string Password = "Correct-Horse-9!";
    private const string IpAddress = "203.0.113.7";

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasherService _passwordHasher = Substitute.For<IPasswordHasherService>();
    private readonly IAuthenticationLogWriter _authLog = Substitute.For<IAuthenticationLogWriter>();
    private readonly List<AuthenticationLog> _entries = [];
    private readonly AuthService _sut;

    public AuthServiceLoginTests()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Users.Returns(_users);
        unitOfWork.RefreshTokens.Returns(Substitute.For<IRefreshTokenRepository>());

        var jwt = Substitute.For<IJwtTokenService>();
        jwt.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("access-token-value");
        jwt.GenerateRefreshToken().Returns("refresh-token-value");
        jwt.HashToken(Arg.Any<string>()).Returns("hashed-token-value");

        _passwordHasher.Verify("stored-hash", Password).Returns(true);
        _authLog.WriteAsync(Arg.Do<AuthenticationLog>(_entries.Add), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        _sut = new AuthService(
            unitOfWork,
            jwt,
            _passwordHasher,
            Substitute.For<IValidator<RegisterRequest>>(),
            new LoginRequestValidator(),
            Substitute.For<IValidator<ForgotPasswordRequest>>(),
            Substitute.For<IValidator<ResetPasswordRequest>>(),
            Substitute.For<IEmailSender>(),
            _authLog,
            Substitute.For<IConfiguration>(),
            Options.Create(new JwtOptions()),
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task Successful_login_records_a_success_entry()
    {
        var user = RegisterUser("jane@example.com");

        var result = await _sut.LoginAsync(new LoginRequest("  Jane@Example.COM ", Password), IpAddress);

        Assert.Equal(user.Id, result.User.Id);
        var entry = Assert.Single(_entries);
        Assert.True(entry.Succeeded);
        Assert.Null(entry.FailureReason);
        Assert.Equal(user.Id, entry.UserId);
        Assert.Equal("jane@example.com", entry.Email);
        Assert.Equal(IpAddress, entry.IpAddress);
        Assert.Equal(TimeSpan.Zero, entry.OccurredAt.Offset);
    }

    [Fact]
    public async Task Unknown_email_records_a_failure_without_a_user()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest("ghost@example.com", Password), IpAddress));

        var entry = Assert.Single(_entries);
        Assert.False(entry.Succeeded);
        Assert.Equal(AuthenticationFailureReason.UnknownAccount, entry.FailureReason);
        Assert.Null(entry.UserId);
        Assert.Equal("ghost@example.com", entry.Email);
        _passwordHasher.DidNotReceiveWithAnyArgs().Verify(default!, default!);
    }

    [Fact]
    public async Task Incorrect_password_records_a_failure_for_the_user()
    {
        var user = RegisterUser("jane@example.com");

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest("jane@example.com", "wrong-password"), IpAddress));

        var entry = Assert.Single(_entries);
        Assert.False(entry.Succeeded);
        Assert.Equal(AuthenticationFailureReason.InvalidPassword, entry.FailureReason);
        Assert.Equal(user.Id, entry.UserId);
    }

    [Fact]
    public async Task Inactive_account_records_a_failure_even_with_the_right_password()
    {
        var user = RegisterUser("jane@example.com", isActive: false);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest("jane@example.com", Password), IpAddress));

        var entry = Assert.Single(_entries);
        Assert.Equal(AuthenticationFailureReason.AccountInactive, entry.FailureReason);
        Assert.Equal(user.Id, entry.UserId);
    }

    [Fact]
    public async Task Unknown_email_and_wrong_password_are_indistinguishable_to_the_caller()
    {
        RegisterUser("jane@example.com");

        var unknown = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest("ghost@example.com", Password), IpAddress));
        var wrongPassword = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest("jane@example.com", "wrong-password"), IpAddress));

        Assert.Equal(unknown.Message, wrongPassword.Message);
    }

    [Theory]
    [InlineData("jane@example.com", Password)]
    [InlineData("jane@example.com", "wrong-password")]
    [InlineData("ghost@example.com", "ghost-password")]
    public async Task Entries_never_contain_credentials_or_tokens(string email, string password)
    {
        RegisterUser("jane@example.com");

        try { await _sut.LoginAsync(new LoginRequest(email, password), IpAddress); }
        catch (UnauthorizedException) { }

        var json = JsonSerializer.Serialize(Assert.Single(_entries));
        Assert.DoesNotContain(password, json);
        Assert.DoesNotContain("stored-hash", json);
        Assert.DoesNotContain("access-token-value", json);
        Assert.DoesNotContain("refresh-token-value", json);
        Assert.DoesNotContain("hashed-token-value", json);
    }

    [Fact]
    public async Task Invalid_request_is_rejected_before_any_attempt_is_recorded()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.LoginAsync(new LoginRequest("not-an-email", Password), IpAddress));

        Assert.Empty(_entries);
    }

    private User RegisterUser(string email, bool isActive = true)
    {
        var user = new User { Email = email, PasswordHash = "stored-hash", FirstName = "Jane", LastName = "Doe", IsActive = isActive };
        user.UserRoles.Add(new UserRole { User = user, Role = new Role { Name = "Customer" } });
        _users.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }
}
