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

    private static Player MakePlayer(string name, params string[] results) => new()
    {
        Name = name,
        Matches = results.Select(r => new MatchResult(Enum.Parse<MatchResult.Result>(r))).ToList()
    };
}
