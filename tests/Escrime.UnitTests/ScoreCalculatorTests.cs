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

    // TC-004
    [Fact]
    [Trait("Requirement", "REQ-E-002")]
    public void CalculateScore_FourConsecutiveWins_AddsBonusOnlyOnce()
    {
        // Arrange
        var matches = ToMatches(["Win", "Win", "Win", "Win"]);

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(17, "because 4*3 = 12 points + a single 5 points bonus for the streak");
    }

    // TC-005
    [Fact]
    [Trait("Requirement", "REQ-E-002")]
    public void CalculateScore_StreakBrokenByLoss_NoBonus()
    {
        // Arrange
        var matches = ToMatches(["Win", "Win", "Loss", "Win"]);

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(9, "because 3+3+0+3 = 9 points and no streak reaches three wins");
    }

    // TC-006
    [Fact]
    [Trait("Requirement", "REQ-E-002")]
    public void CalculateScore_StreakBrokenByDraw_NoBonus()
    {
        // Arrange
        var matches = ToMatches(["Win", "Draw", "Win", "Win"]);

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(10, "because 3+1+3+3 = 10 points and the draw breaks the streak");
    }

    // TC-007
    public static TheoryData<string[], int, string> MultipleStreakCases => new()
    {
        // Exemple 3 du sujet : 21 points + 5 (série de 3) + 5 (série de 4) — cf. H1
        { ["Win", "Win", "Win", "Loss", "Win", "Win", "Win", "Win"], 31, "exemple 3 du sujet (H1)" },
        // « Win-Loss-Win-Win-Win » : bonus accordé pour les 3 dernières victoires
        { ["Win", "Loss", "Win", "Win", "Win"], 17, "bonus pour les 3 dernières victoires" },
        // Deux séries de 3 exactement
        { ["Win", "Win", "Win", "Loss", "Win", "Win", "Win"], 28, "deux séries de 3" }
    };

    [Theory]
    [Trait("Requirement", "REQ-E-002")]
    [MemberData(nameof(MultipleStreakCases))]
    public void CalculateScore_MultipleWinStreaks_AddsBonusPerStreak(string[] results, int expectedScore, string because)
    {
        // Arrange
        var matches = ToMatches(results);

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(expectedScore, because);
    }

    // TC-008
    [Fact]
    [Trait("Requirement", "REQ-E-003")]
    public void CalculateScore_DisqualifiedWithPositiveScore_ReturnsZero()
    {
        // Arrange
        var matches = ToMatches(["Win", "Win", "Win"]); // 14 points sans disqualification

        // Act
        var score = _calculator.CalculateScore(matches, isDisqualified: true);

        // Assert
        score.Should().Be(0, "because a disqualified player loses everything regardless of past performance");
    }

    // TC-009
    [Fact]
    [Trait("Requirement", "REQ-E-003")]
    public void CalculateScore_DisqualifiedWithoutMatches_ReturnsZero()
    {
        // Arrange
        var matches = new List<MatchResult>();

        // Act
        var score = _calculator.CalculateScore(matches, isDisqualified: true);

        // Assert
        score.Should().Be(0);
    }

    // TC-010
    [Fact]
    [Trait("Requirement", "REQ-E-004")]
    public void CalculateScore_PenaltiesSubtracted_ReturnsReducedScore()
    {
        // Arrange
        var matches = ToMatches(["Win", "Win", "Draw", "Win"]); // 10 points, pas de bonus

        // Act
        var score = _calculator.CalculateScore(matches, penaltyPoints: 3);

        // Assert
        score.Should().Be(7, "because 10 points - 3 penalty points = 7");
    }

    private static List<MatchResult> ToMatches(IEnumerable<string> results) =>
        results.Select(r => new MatchResult(Enum.Parse<MatchResult.Result>(r))).ToList();
}
