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

    private static List<MatchResult> ToMatches(IEnumerable<string> results) =>
        results.Select(r => new MatchResult(Enum.Parse<MatchResult.Result>(r))).ToList();
}
