using Escrime.Domain;
using FluentAssertions;

namespace Escrime.UnitTests;

public class ScoreBreakdownTests
{
    private readonly ScoreCalculator _calculator = new();

    private static List<MatchResult> ToMatches(string[] results) =>
        results.Select(r => new MatchResult(Enum.Parse<MatchResult.Result>(r))).ToList();

    // TC-030 — scénario de la spec front (section 5) : série de trois puis nul
    [Fact]
    [Trait("Requirement", "REQ-E-018")]
    public void CalculateBreakdown_ThreeWinsThenDraw_TellsTheFullStory()
    {
        // Arrange
        var matches = ToMatches(new[] { "Win", "Win", "Win", "Draw" });

        // Act
        var breakdown = _calculator.CalculateBreakdown(matches);

        // Assert
        breakdown.FinalScore.Should().Be(15, "because 3*3 + 5 streak bonus + 1 draw = 15");
        breakdown.IsDisqualified.Should().BeFalse();
        breakdown.Events.Should().Equal(
            new MatchScoredEvent(0, MatchResult.Result.Win, 3, 3),
            new MatchScoredEvent(1, MatchResult.Result.Win, 3, 6),
            new MatchScoredEvent(2, MatchResult.Result.Win, 3, 9),
            new StreakBonusEvent(2, 5, 14),
            new MatchScoredEvent(3, MatchResult.Result.Draw, 1, 15));
    }

    // TC-031 — la pénalité draine le score puis bute au plancher zéro
    [Fact]
    [Trait("Requirement", "REQ-E-018")]
    public void CalculateBreakdown_PenaltyBelowZero_EmitsClampToZero()
    {
        // Arrange
        var matches = ToMatches(new[] { "Win" });

        // Act
        var breakdown = _calculator.CalculateBreakdown(matches, penaltyPoints: 5);

        // Assert
        breakdown.FinalScore.Should().Be(0, "because the score never goes below zero");
        breakdown.Events.Should().Equal(
            new MatchScoredEvent(0, MatchResult.Result.Win, 3, 3),
            new PenaltyEvent(-5, -2),
            new ClampToZeroEvent(0));
    }

    // TC-032 — la disqualification scelle le déroulé à zéro, l'histoire reste racontée
    [Fact]
    [Trait("Requirement", "REQ-E-018")]
    public void CalculateBreakdown_Disqualified_EndsWithDisqualificationAtZero()
    {
        // Arrange
        var matches = ToMatches(new[] { "Win", "Win" });

        // Act
        var breakdown = _calculator.CalculateBreakdown(matches, isDisqualified: true);

        // Assert
        breakdown.FinalScore.Should().Be(0, "because disqualification cancels everything");
        breakdown.IsDisqualified.Should().BeTrue();
        breakdown.Events.Should().EndWith(new DisqualificationEvent(0));
        breakdown.Events.OfType<MatchScoredEvent>().Should().HaveCount(2);
    }

    // TC-033 — cohérence : le déroulé atterrit exactement sur CalculateScore
    [Theory]
    [Trait("Requirement", "REQ-E-018")]
    [InlineData(new[] { "Win", "Win", "Win", "Win" }, 0, false)]
    [InlineData(new[] { "Win", "Draw", "Loss", "Win" }, 3, false)]
    [InlineData(new[] { "Win" }, 5, false)]
    [InlineData(new[] { "Win", "Win", "Win" }, 2, true)]
    [InlineData(new string[0], 0, false)]
    [InlineData(new[] { "Win", "Win", "Win", "Loss", "Win", "Win", "Win" }, 1, false)]
    public void CalculateBreakdown_AnyScenario_LandsOnCalculateScore(string[] results, int penalty, bool disqualified)
    {
        // Arrange
        var matches = ToMatches(results);

        // Act
        var breakdown = _calculator.CalculateBreakdown(matches, disqualified, penalty);
        var score = _calculator.CalculateScore(matches, disqualified, penalty);

        // Assert
        breakdown.FinalScore.Should().Be(score, "because both walk the exact same rules");
        if (breakdown.Events.Count > 0)
            breakdown.Events[^1].RunningScore.Should().Be(score, "because the story ends on the final score");
    }

    // TC-034 — mêmes garde-fous que CalculateScore
    [Fact]
    [Trait("Requirement", "REQ-E-018")]
    public void CalculateBreakdown_InvalidInputs_ThrowLikeCalculateScore()
    {
        // Act
        var nullMatches = () => _calculator.CalculateBreakdown(null!);
        var negativePenalty = () => _calculator.CalculateBreakdown(new List<MatchResult>(), penaltyPoints: -1);

        // Assert
        nullMatches.Should().Throw<ArgumentNullException>();
        negativePenalty.Should().Throw<ArgumentException>();
    }
}
