using Escrime.Domain;

namespace Escrime.Api.Dtos;

public record PlayerDto(int Id, string Name, int Score, bool IsDisqualified, int PenaltyPoints, int MatchesPlayed);

public record MatchDto(int Id, MatchResult.Result Result);

public record PlayerDetailDto(int Id, string Name, int Score, bool IsDisqualified, int PenaltyPoints, List<MatchDto> Matches);

public record RankingEntryDto(int Rank, int PlayerId, string Name, int Score);
