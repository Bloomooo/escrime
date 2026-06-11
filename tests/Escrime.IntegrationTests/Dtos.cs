namespace Escrime.IntegrationTests;

// Formes de réponse attendues de l'API (contrat vu du client / futur front)
public record PlayerDto(int Id, string Name, int Score, bool IsDisqualified, int PenaltyPoints, int MatchesPlayed);
public record MatchDto(int Id, string Result);
public record PlayerDetailDto(int Id, string Name, int Score, bool IsDisqualified, int PenaltyPoints, List<MatchDto> Matches);
public record RankingEntryDto(int Rank, int PlayerId, string Name, int Score);
public record ScoreEventDto(string Type, int? Index, string? Outcome, int? AfterMatchIndex, int? Points, int RunningScore);
public record ScoreBreakdownDto(int FinalScore, bool IsDisqualified, List<ScoreEventDto> Events);
