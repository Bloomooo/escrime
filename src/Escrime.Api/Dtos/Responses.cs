using System.Text.Json.Serialization;
using Escrime.Domain;

namespace Escrime.Api.Dtos;

public record PlayerDto(int Id, string Name, int Score, bool IsDisqualified, int PenaltyPoints, int MatchesPlayed);

public record MatchDto(int Id, MatchResult.Result Result);

public record PlayerDetailDto(int Id, string Name, int Score, bool IsDisqualified, int PenaltyPoints, List<MatchDto> Matches);

public record RankingEntryDto(int Rank, int PlayerId, string Name, int Score);

// Forme exacte attendue par le front (spec front, section 5) :
// les champs absents d'un type d'événement sont omis du JSON.
public record ScoreEventDto(
    string Type,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? Index,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] MatchResult.Result? Outcome,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? AfterMatchIndex,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? Points,
    int RunningScore);

public record ScoreBreakdownDto(int FinalScore, bool IsDisqualified, List<ScoreEventDto> Events);
