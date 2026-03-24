using CleanArchTemplate.Application.Auth.Commands;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Domain.Entities;
using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CleanArchTemplate.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly Mock<IPasswordService> _passwordMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _jwtMock = new Mock<IJwtTokenService>();
        _passwordMock = new Mock<IPasswordService>();

        _handler = new LoginCommandHandler(
            _contextMock.Object,
            _jwtMock.Object,
            _passwordMock.Object);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var emptyUsers = new List<User>().AsQueryable();
        var dbSetMock = CreateMockDbSet(emptyUsers);
        _contextMock.Setup(c => c.Users).Returns(dbSetMock.Object);
        _passwordMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var command = new LoginCommand { Email = "nobody@example.com", Password = "wrong" };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == "AUTH_001");
    }

    [Fact]
    public async Task Handle_WithSuspendedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Email = "suspended@example.com",
            PasswordHash = "hash",
            Status = UserStatus.Suspended,
            IsVerified = true,
            UserRoles = new List<UserRole>()
        };

        var users = new List<User> { user }.AsQueryable();
        var dbSetMock = CreateMockDbSet(users);
        _contextMock.Setup(c => c.Users).Returns(dbSetMock.Object);
        _passwordMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), "hash")).Returns(true);

        var command = new LoginCommand { Email = "suspended@example.com", Password = "correct" };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == "AUTH_005");
    }

    [Fact]
    public async Task Handle_WithUnverifiedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Email = "unverified@example.com",
            PasswordHash = "hash",
            Status = UserStatus.Active,
            IsVerified = false,
            UserRoles = new List<UserRole>()
        };

        var users = new List<User> { user }.AsQueryable();
        var dbSetMock = CreateMockDbSet(users);
        _contextMock.Setup(c => c.Users).Returns(dbSetMock.Object);
        _passwordMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), "hash")).Returns(true);

        var command = new LoginCommand { Email = "unverified@example.com", Password = "correct" };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == "AUTH_004");
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mock = new Mock<DbSet<T>>();
        mock.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        mock.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mock;
    }
}
