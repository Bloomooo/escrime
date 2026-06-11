using Escrime.Domain;
using FluentAssertions;

namespace Escrime.UnitTests;

public class ScoreCalculatorTests
{
    private readonly ScoreCalculator _calculator = new();

    // TC-001
    [Fact]
    [Trait("Requirement", "REQ-E-001")]
    public void CalculateScore_WinDrawLoss_Returns4Points()
    {
        // Arrange
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),  // 3 points
            new(MatchResult.Result.Draw), // 1 point
            new(MatchResult.Result.Loss)  // 0 point
        };

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(4, "because 3+1+0 = 4 points without bonus");
    }

    // TC-002
    [Theory]
    [Trait("Requirement", "REQ-E-001")]
    [InlineData(new[] { "Win", "Win" }, 6)]                  // que des victoires
    [InlineData(new[] { "Draw", "Draw", "Draw" }, 3)]        // que des nuls
    [InlineData(new[] { "Loss", "Loss" }, 0)]                // que des défaites
    [InlineData(new[] { "Win", "Draw", "Loss", "Win" }, 7)]  // exemple 1 du sujet
    public void CalculateScore_BasicCombinations_ReturnsExpectedScore(string[] results, int expectedScore)
    {
        // Arrange
        var matches = ToMatches(results);

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(expectedScore);
    }

    // TC-013
    [Fact]
    [Trait("Requirement", "REQ-E-006")]
    public void CalculateScore_EmptyMatches_ReturnsZero()
    {
        // Arrange
        var matches = new List<MatchResult>();

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(0, "because a player without any match has no points");
    }

    // TC-014
    [Fact]
    [Trait("Requirement", "REQ-E-007")]
    public void CalculateScore_NullMatches_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _calculator.CalculateScore(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("matches")
            .WithMessage("*cannot be null*");
    }

    // TC-003
    [Fact]
    [Trait("Requirement", "REQ-E-002")]
    public void CalculateScore_ThreeConsecutiveWins_Adds5PointsBonus()
    {
        // Arrange
        var matches = ToMatches(["Win", "Win", "Win"]);

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(14, "because 3*3 = 9 points + 5 bonus for three consecutive wins");
    }

    private static List<MatchResult> ToMatches(IEnumerable<string> results) =>
        results.Select(r => new MatchResult(Enum.Parse<MatchResult.Result>(r))).ToList();
}
