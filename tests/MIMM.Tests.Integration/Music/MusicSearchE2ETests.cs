using System.Net.Http.Json;
using FluentAssertions;
using MIMM.Shared.Dtos;
using MIMM.Tests.Integration.Helpers;
using Xunit;

namespace MIMM.Tests.Integration.Music;

public class MusicSearchE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MusicSearchE2ETests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MusicSearch_ReturnsResults()
    {
        var response = await _client.GetAsync("/api/music/search?query=coldplay&limit=3");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<MusicSearchResponse>();
        payload.Should().NotBeNull();
        payload!.Items.Should().NotBeNull();
        payload.Items.Count.Should().BeLessOrEqualTo(3);
    }
}
