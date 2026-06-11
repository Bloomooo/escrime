using Escrime.Api.Data.Entities;
using Escrime.Domain;

namespace Escrime.Api.Data;

/// <summary>
/// Conversion entité de persistance → modèle du domaine, pour alimenter
/// ScoreCalculator et TournamentRanking sans polluer le domaine avec EF.
/// </summary>
public static class PlayerMapper
{
    public static Player ToDomain(PlayerEntity entity) => new()
    {
        Name = entity.Name,
        IsDisqualified = entity.IsDisqualified,
        PenaltyPoints = entity.PenaltyPoints,
        Matches = entity.Matches
            .OrderBy(m => m.Id) // ordre chronologique, le bonus de série en dépend
            .Select(m => new MatchResult(m.Result))
            .ToList()
    };
}
