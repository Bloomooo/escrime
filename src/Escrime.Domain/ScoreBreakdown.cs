namespace Escrime.Domain;

/// <summary>
/// Déroulé du score, événement par événement, dans l'ordre où les règles
/// s'appliquent. Le front le rejoue tel quel et ne recalcule jamais.
/// </summary>
public abstract record ScoreEvent(int RunningScore);

public sealed record MatchScoredEvent(int Index, MatchResult.Result Outcome, int Points, int RunningScore)
    : ScoreEvent(RunningScore);

public sealed record StreakBonusEvent(int AfterMatchIndex, int Points, int RunningScore)
    : ScoreEvent(RunningScore);

public sealed record PenaltyEvent(int Points, int RunningScore)
    : ScoreEvent(RunningScore);

public sealed record ClampToZeroEvent(int RunningScore)
    : ScoreEvent(RunningScore);

public sealed record DisqualificationEvent(int RunningScore)
    : ScoreEvent(RunningScore);

public sealed record ScoreBreakdown(int FinalScore, bool IsDisqualified, IReadOnlyList<ScoreEvent> Events);
