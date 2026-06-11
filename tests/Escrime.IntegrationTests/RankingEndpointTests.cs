using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Escrime.IntegrationTests;

public class RankingEndpointTests
{
    private static async Task<PlayerDto> CreatePlayerWithResults(HttpClient client, string name, params string[] results)
    {
        var response = await client.PostAsJsonAsync("/api/players", new { name });
        var player = (await response.Content.ReadFromJsonAsync<PlayerDto>())!;
        foreach (var result in results)
            await client.PostAsJsonAsync($"/api/players/{player.Id}/matches", new { result });
        return player;
    }

    // TC-120
    [Fact]
    [Trait("Requirement", "REQ-E-017")]
    public async Task GetRanking_ThreePlayers_SortedByScoreDescendingWithRanks()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        await CreatePlayerWithResults(client, "Sir Galahad", "Win", "Draw");            // 4 points
        await CreatePlayerWithResults(client, "Dame Morgane", "Win", "Win", "Win");     // 14 points
        await CreatePlayerWithResults(client, "Chevalier Noir", "Loss");                // 0 point

        // Act
        var ranking = await client.GetFromJsonAsync<List<RankingEntryDto>>("/api/ranking");

        // Assert
        ranking!.Select(r => r.Name).Should().ContainInOrder("Dame Morgane", "Sir Galahad", "Chevalier Noir");
        ranking.Select(r => r.Score).Should().ContainInOrder(14, 4, 0);
        ranking.Select(r => r.Rank).Should().ContainInOrder(1, 2, 3);
    }

    // TC-121
    [Fact]
    [Trait("Requirement", "REQ-E-017")]
    public async Task GetChampion_ReturnsHighestScorer()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        await CreatePlayerWithResults(client, "Sir Galahad", "Win");                    // 3 points
        await CreatePlayerWithResults(client, "Dame Morgane", "Win", "Win", "Win");     // 14 points

        // Act
        var champion = await client.GetFromJsonAsync<RankingEntryDto>("/api/ranking/champion");

        // Assert
        champion!.Name.Should().Be("Dame Morgane");
        champion.Score.Should().Be(14);
        champion.Rank.Should().Be(1);
    }

    // TC-122
    [Fact]
    [Trait("Requirement", "REQ-E-017")]
    public async Task GetChampion_NoPlayers_Returns404()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/ranking/champion");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
