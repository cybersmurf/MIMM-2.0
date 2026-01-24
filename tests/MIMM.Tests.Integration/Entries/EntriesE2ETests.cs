using System.Net.Http.Json;
using FluentAssertions;
using MIMM.Shared.Dtos;
using MIMM.Tests.Integration.Helpers;
using Xunit;

namespace MIMM.Tests.Integration.Entries;

public class EntriesE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EntriesE2ETests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Entries_CRUD_Flow_Succeeds()
    {
        // Arrange: create and login a user
        var email = $"e2e_entries_{Guid.NewGuid():N}@example.com";
        var password = "Test123!";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new { Email = email, Password = password, DisplayName = "Entries User" });
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<MIMM.Shared.Dtos.AuthenticationResponse>();
        // Authenticate via test scheme using user id
        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", login!.User.Id.ToString());

        // Create entry
        var create = new CreateEntryRequest
        {
            SongTitle = "Yellow",
            ArtistName = "Coldplay",
            AlbumName = "Parachutes",
            Valence = 0.6,
            Arousal = 0.3,
            TensionLevel = 40,
            SomaticTags = new[] { "Relaxed", "Warm" },
            Notes = "Great mood"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/entries", create);
        if (!createResponse.IsSuccessStatusCode)
        {
            var body = await createResponse.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Create failed: {(int)createResponse.StatusCode} {createResponse.StatusCode} - {body}");
        }
        var created = await createResponse.Content.ReadFromJsonAsync<EntryDto>();
        created.Should().NotBeNull();
        var entryId = created!.Id;

        // List entries
        var listResponse = await _client.GetAsync("/api/entries?pageNumber=1&pageSize=10&sortBy=created&sortDirection=desc");
        if (!listResponse.IsSuccessStatusCode)
        {
            var body = await listResponse.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"List failed: {(int)listResponse.StatusCode} {listResponse.StatusCode} - {body}");
        }
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResponse<EntryDto>>();
        list.Should().NotBeNull();
        list!.Items.Should().ContainSingle(e => e.Id == entryId);

        // Get by id
        var getResponse = await _client.GetAsync($"/api/entries/{entryId}");
        if (!getResponse.IsSuccessStatusCode)
        {
            var body = await getResponse.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Get failed: {(int)getResponse.StatusCode} {getResponse.StatusCode} - {body}");
        }
        var fetched = await getResponse.Content.ReadFromJsonAsync<EntryDto>();
        fetched!.SongTitle.Should().Be("Yellow");

        // Update
        var update = new UpdateEntryRequest
        {
            SongTitle = "Fix You",
            Valence = 0.7,
            Arousal = 0.4,
            TensionLevel = 35
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/entries/{entryId}", update);
        if (!updateResponse.IsSuccessStatusCode)
        {
            var body = await updateResponse.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Update failed: {(int)updateResponse.StatusCode} {updateResponse.StatusCode} - {body}");
        }
        var updated = await updateResponse.Content.ReadFromJsonAsync<EntryDto>();
        updated!.SongTitle.Should().Be("Fix You");
        updated.Valence.Should().BeApproximately(0.7, 1e-6);
        updated.Arousal.Should().BeApproximately(0.4, 1e-6);
        updated.TensionLevel.Should().Be(35);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/entries/{entryId}");
        if (!deleteResponse.IsSuccessStatusCode)
        {
            var body = await deleteResponse.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Delete failed: {(int)deleteResponse.StatusCode} {deleteResponse.StatusCode} - {body}");
        }

        // List again should not contain the deleted entry (soft delete filtered)
        var listAfterDelete = await _client.GetAsync("/api/entries?pageNumber=1&pageSize=10&sortBy=created&sortDirection=desc");
        listAfterDelete.EnsureSuccessStatusCode();
        var list2 = await listAfterDelete.Content.ReadFromJsonAsync<PagedResponse<EntryDto>>();
        list2!.Items.Should().NotContain(e => e.Id == entryId);
    }

    // Use shared DTOs
}
