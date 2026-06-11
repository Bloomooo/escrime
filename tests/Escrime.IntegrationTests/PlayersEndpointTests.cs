using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Escrime.IntegrationTests;

public class PlayersEndpointTests
{
    // TC-101
    [Fact]
    [Trait("Requirement", "REQ-E-015")]
    public async Task PostPlayer_ValidName_Returns201WithLocationAndBody()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/players", new { name = "Sir Galahad" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var player = await response.Content.ReadFromJsonAsync<PlayerDto>();
        player!.Name.Should().Be("Sir Galahad");
        player.Score.Should().Be(0);
        player.Id.Should().BePositive();
    }

    // TC-102
    [Fact]
    [Trait("Requirement", "REQ-E-015")]
    public async Task PostPlayer_EmptyName_Returns400()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/players", new { name = "" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // TC-103
    [Fact]
    [Trait("Requirement", "REQ-E-015")]
    public async Task GetPlayer_UnknownId_Returns404()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/players/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // TC-104
    [Fact]
    [Trait("Requirement", "REQ-E-015")]
    public async Task DeletePlayer_ExistingPlayer_Returns204ThenGetReturns404()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/players", new { name = "Dame Morgane" }))
            .Content.ReadFromJsonAsync<PlayerDto>();

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/players/{created!.Id}");
        var getResponse = await client.GetAsync($"/api/players/{created.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // TC-105
    [Fact]
    [Trait("Requirement", "REQ-E-015")]
    public async Task GetPlayers_TwoCreated_ReturnsBoth()
    {
        // Arrange
        using var factory = new EscrimeApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/players", new { name = "Sir Galahad" });
        await client.PostAsJsonAsync("/api/players", new { name = "Dame Morgane" });

        // Act
        var players = await client.GetFromJsonAsync<List<PlayerDto>>("/api/players");

        // Assert
        players.Should().HaveCount(2);
        players!.Select(p => p.Name).Should().Contain(["Sir Galahad", "Dame Morgane"]);
    }
}
