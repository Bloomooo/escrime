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
}
