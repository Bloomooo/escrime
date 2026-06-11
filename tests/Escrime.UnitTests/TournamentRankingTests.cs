using Escrime.Domain;
using FluentAssertions;

namespace Escrime.UnitTests;

public class TournamentRankingTests
{
    private readonly TournamentRanking _ranking = new(new ScoreCalculator());

    // TC-017
    [Fact]
    [Trait("Requirement", "REQ-E-010")]
    public void GetRanking_PlayersWithDifferentScores_SortsByScoreDescending()
    {
        // Arrange
        var galahad = MakePlayer("Sir Galahad", "Win", "Draw");            // 4 points
        var morgane = MakePlayer("Dame Morgane", "Win", "Win", "Win");     // 14 points
        var noir = MakePlayer("Chevalier Noir", "Loss", "Loss");           // 0 point
        var players = new List<Player> { galahad, morgane, noir };

        // Act
        var ranking = _ranking.GetRanking(players);

        // Assert
        ranking.Should().ContainInOrder(morgane, galahad, noir);
    }

    // TC-018
    [Fact]
    [Trait("Requirement", "REQ-E-011")]
    public void GetRanking_TiedScores_PreservesInsertionOrder()
    {
        // Arrange : Galahad et Morgane ex æquo (7 points), Noir derrière (1 point)
        var galahad = MakePlayer("Sir Galahad", "Win", "Win", "Draw");   // 7 points
        var morgane = MakePlayer("Dame Morgane", "Win", "Draw", "Win"); // 7 points
        var noir = MakePlayer("Chevalier Noir", "Draw");                 // 1 point
        var players = new List<Player> { galahad, morgane, noir };

        // Act
        var ranking = _ranking.GetRanking(players);

        // Assert : tri stable (H2), les ex æquo gardent leur ordre d'entrée
        ranking.Should().ContainInOrder(galahad, morgane, noir);
    }

    // TC-019
    [Fact]
    [Trait("Requirement", "REQ-E-012")]
    public void GetChampion_MultiplePlayers_ReturnsHighestScorer()
    {
        // Arrange
        var galahad = MakePlayer("Sir Galahad", "Win", "Draw");        // 4 points
        var morgane = MakePlayer("Dame Morgane", "Win", "Win", "Win"); // 14 points
        var noir = MakePlayer("Chevalier Noir", "Win", "Win", "Draw"); // 7 points
        var players = new List<Player> { galahad, morgane, noir };

        // Act
        var champion = _ranking.GetChampion(players);

        // Assert
        champion.Should().BeSameAs(morgane, "because 14 points is the highest score");
    }

    private static Player MakePlayer(string name, params string[] results) => new()
    {
        Name = name,
        Matches = results.Select(r => new MatchResult(Enum.Parse<MatchResult.Result>(r))).ToList()
    };
}
