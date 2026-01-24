using System.Net.Http.Json;
using FluentAssertions;
using MIMM.Tests.Integration.Helpers;
using Xunit;

namespace MIMM.Tests.Integration.Auth;

public class AuthE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthE2ETests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_Me_Flow_Succeeds()
    {
        var email = $"e2e_{Guid.NewGuid():N}@example.com";
        var password = "Test123!";

        // Register
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = password,
            DisplayName = "E2E User"
        });
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<MIMM.Shared.Dtos.AuthenticationResponse>();
        login.Should().NotBeNull();
        login!.AccessToken.Should().NotBeNullOrWhiteSpace();
        // Use test auth header to authenticate requests in Testing environment
        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", login.User.Id.ToString());

        // /me
        var meResponse = await _client.GetAsync("/api/auth/me");
        if (!meResponse.IsSuccessStatusCode)
        {
            var body = await meResponse.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"/api/auth/me failed: {(int)meResponse.StatusCode} {meResponse.StatusCode} - {body}");
        }

        var me = await meResponse.Content.ReadFromJsonAsync<MIMM.Shared.Dtos.UserDto>();
        me.Should().NotBeNull();
        me!.Email.Should().Be(email);
        me.DisplayName.Should().Be("E2E User");
    }

    // Use shared DTOs
}
