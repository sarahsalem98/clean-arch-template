using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CleanArchTemplate.IntegrationTests.Auth;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with InMemory for tests
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid()));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var payload = new
        {
            email = "integrationtest@example.com",
            password = "Test@1234!",
            firstName = "Integration",
            lastName = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/user/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOpts);
        body!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns422()
    {
        var payload = new
        {
            email = "not-an-email",
            password = "Test@1234!",
            firstName = "Bad",
            lastName = "Email"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/user/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithUnknownUser_Returns401()
    {
        var payload = new
        {
            email = "nobody@example.com",
            password = "Test@1234!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/user/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var payload = new
        {
            email = "dup@example.com",
            password = "Test@1234!",
            firstName = "Dup",
            lastName = "User"
        };

        await _client.PostAsJsonAsync("/api/v1/user/auth/register", payload);
        var response = await _client.PostAsJsonAsync("/api/v1/user/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
