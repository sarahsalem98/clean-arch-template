using CleanArchTemplate.Application.Auth.Commands;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CleanArchTemplate.UnitTests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly Mock<IPasswordService> _passwordMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _jwtMock = new Mock<IJwtTokenService>();
        _passwordMock = new Mock<IPasswordService>();
        _emailMock = new Mock<IEmailService>();

        _handler = new RegisterCommandHandler(
            _contextMock.Object,
            _jwtMock.Object,
            _passwordMock.Object,
            _emailMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var existingUsers = new List<User>
        {
            new() { Email = "existing@example.com", IsDeleted = false }
        }.AsQueryable();

        var userDbSetMock = CreateMockDbSet(existingUsers);
        _contextMock.Setup(c => c.Users).Returns(userDbSetMock.Object);

        var command = new RegisterCommand
        {
            Email = "existing@example.com",
            Password = "Test@1234",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.HttpStatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var emptyUsers = new List<User>().AsQueryable();
        var userDbSetMock = CreateMockDbSet(emptyUsers);

        var emptyRoles = new List<Role>
        {
            new() { Id = Guid.NewGuid(), Name = "User" }
        }.AsQueryable();
        var roleDbSetMock = CreateMockDbSet(emptyRoles);

        var refreshTokens = new List<RefreshToken>().AsQueryable();
        var refreshTokenDbSetMock = CreateMockDbSet(refreshTokens);

        var deviceTokens = new List<DeviceToken>().AsQueryable();
        var deviceTokenDbSetMock = CreateMockDbSet(deviceTokens);

        var userRoles = new List<UserRole>().AsQueryable();
        var userRoleDbSetMock = CreateMockDbSet(userRoles);

        _contextMock.Setup(c => c.Users).Returns(userDbSetMock.Object);
        _contextMock.Setup(c => c.Roles).Returns(roleDbSetMock.Object);
        _contextMock.Setup(c => c.RefreshTokens).Returns(refreshTokenDbSetMock.Object);
        _contextMock.Setup(c => c.DeviceTokens).Returns(deviceTokenDbSetMock.Object);
        _contextMock.Setup(c => c.UserRoles).Returns(userRoleDbSetMock.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed");
        _jwtMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), It.IsAny<string>(),
            It.IsAny<IList<string>>(), It.IsAny<IList<string>>())).Returns("access-token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _emailMock.Setup(e => e.SendVerificationEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RegisterCommand
        {
            Email = "new@example.com",
            Password = "Test@1234",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.HttpStatusCode.Should().Be(201);
        result.Value.Should().NotBeNull();
        result.Value!.Tokens.AccessToken.Should().Be("access-token");
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
