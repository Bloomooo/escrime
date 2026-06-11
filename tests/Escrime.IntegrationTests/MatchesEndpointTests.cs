using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Escrime.IntegrationTests;

public class MatchesEndpointTests
{
    private static async Task<PlayerDto> CreatePlayer(HttpClient client, string name = "Sir Galahad")
    {
        var response = await client.PostAsJsonAsync("/api/players", new { name });
        return (await response.Content.ReadFromJsonAsync<PlayerDto>())!;
    }

    // TC-110 — test clé : le bonus de série traverse la pile HTTP + EF,
    // preuve que l'ordre chronologique des combats est préservé en base.
    [Fact]
    [Trait("Requirement", "REQ-E-016")]
    public async Task PostFourWins_ThenGetPlayer_ScoreIs17WithStreakBonus()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        var player = await CreatePlayer(client);

        // Act : 4 victoires enregistrées une par une
        for (var i = 0; i < 4; i++)
        {
            var matchResponse = await client.PostAsJsonAsync($"/api/players/{player.Id}/matches", new { result = "Win" });
            matchResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }
        var detail = await client.GetFromJsonAsync<PlayerDetailDto>($"/api/players/{player.Id}");

        // Assert
        detail!.Score.Should().Be(17, "because 4*3 = 12 + 5 streak bonus, computed across HTTP and EF");
        detail.Matches.Should().HaveCount(4);
    }

    // TC-111
    [Fact]
    [Trait("Requirement", "REQ-E-016")]
    public async Task PostMatch_InvalidResult_Returns400()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        var player = await CreatePlayer(client);

        // Act
        var response = await client.PostAsJsonAsync($"/api/players/{player.Id}/matches", new { result = "Banana" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // TC-112
    [Fact]
    [Trait("Requirement", "REQ-E-016")]
    public async Task PostPenalties_ReducesScore_AndNegativeReturns400()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        var player = await CreatePlayer(client);
        await client.PostAsJsonAsync($"/api/players/{player.Id}/matches", new { result = "Win" });   // 3 points

        // Act
        var penaltyResponse = await client.PostAsJsonAsync($"/api/players/{player.Id}/penalties", new { points = 2 });
        var negativeResponse = await client.PostAsJsonAsync($"/api/players/{player.Id}/penalties", new { points = -1 });
        var updated = await penaltyResponse.Content.ReadFromJsonAsync<PlayerDto>();

        // Assert
        penaltyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Score.Should().Be(1, "because 3 - 2 penalty points = 1");
        negativeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // TC-113
    [Fact]
    [Trait("Requirement", "REQ-E-016")]
    public async Task PostDisqualification_ScoreBecomesZero()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        var player = await CreatePlayer(client);
        for (var i = 0; i < 3; i++)
            await client.PostAsJsonAsync($"/api/players/{player.Id}/matches", new { result = "Win" }); // 14 points

        // Act
        var response = await client.PostAsync($"/api/players/{player.Id}/disqualification", null);
        var updated = await response.Content.ReadFromJsonAsync<PlayerDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.IsDisqualified.Should().BeTrue();
        updated.Score.Should().Be(0, "because disqualification cancels everything");
    }
}
