using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Escrime.IntegrationTests;

public class BreakdownEndpointTests
{
    private static async Task<PlayerDto> CreatePlayer(HttpClient client, string name = "Sir Galahad")
    {
        var response = await client.PostAsJsonAsync("/api/players", new { name });
        return (await response.Content.ReadFromJsonAsync<PlayerDto>())!;
    }

    // TC-130 — le déroulé complet (série, nul, pénalité) traverse HTTP + EF
    [Fact]
    [Trait("Requirement", "REQ-E-018")]
    public async Task GetScoreBreakdown_StreakDrawAndPenalty_ReplaysTheExactStory()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        var player = await CreatePlayer(client);
        foreach (var result in new[] { "Win", "Win", "Win", "Draw" })
            await client.PostAsJsonAsync($"/api/players/{player.Id}/matches", new { result });
        await client.PostAsJsonAsync($"/api/players/{player.Id}/penalties", new { points = 3 });

        // Act
        var breakdown = await client.GetFromJsonAsync<ScoreBreakdownDto>($"/api/players/{player.Id}/score-breakdown");

        // Assert
        breakdown!.FinalScore.Should().Be(12, "because 15 from the matches minus 3 penalty points = 12");
        breakdown.IsDisqualified.Should().BeFalse();
        breakdown.Events.Select(e => e.Type).Should().Equal(
            "match", "match", "match", "streakBonus", "match", "penalty");
        breakdown.Events[0].Outcome.Should().Be("Win");
        breakdown.Events[3].AfterMatchIndex.Should().Be(2);
        breakdown.Events[3].Points.Should().Be(5);
        breakdown.Events[3].RunningScore.Should().Be(14);
        breakdown.Events[4].Outcome.Should().Be("Draw");
        breakdown.Events[5].Points.Should().Be(-3);
        breakdown.Events[5].RunningScore.Should().Be(12);
    }

    // TC-131 — joueur inconnu → 404
    [Fact]
    [Trait("Requirement", "REQ-E-018")]
    public async Task GetScoreBreakdown_UnknownPlayer_Returns404()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/players/999/score-breakdown");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // TC-132 — disqualifié : le déroulé se scelle sur l'événement disqualification
    [Fact]
    [Trait("Requirement", "REQ-E-018")]
    public async Task GetScoreBreakdown_DisqualifiedPlayer_EndsAtZero()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        var player = await CreatePlayer(client);
        await client.PostAsJsonAsync($"/api/players/{player.Id}/matches", new { result = "Win" });
        await client.PostAsync($"/api/players/{player.Id}/disqualification", null);

        // Act
        var breakdown = await client.GetFromJsonAsync<ScoreBreakdownDto>($"/api/players/{player.Id}/score-breakdown");

        // Assert
        breakdown!.FinalScore.Should().Be(0, "because disqualification cancels everything");
        breakdown.IsDisqualified.Should().BeTrue();
        breakdown.Events.Last().Type.Should().Be("disqualification");
        breakdown.Events.Last().RunningScore.Should().Be(0);
    }
}
