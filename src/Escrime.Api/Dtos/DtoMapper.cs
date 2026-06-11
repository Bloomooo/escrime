using Escrime.Api.Data;
using Escrime.Api.Data.Entities;
using Escrime.Domain;

namespace Escrime.Api.Dtos;

public static class DtoMapper
{
    /// <summary>
    /// Projette une entité en DTO ; le score n'est jamais stocké, il est
    /// recalculé ici par ScoreCalculator à chaque lecture.
    /// </summary>
    public static PlayerDto ToDto(PlayerEntity entity, ScoreCalculator calculator)
    {
        var domain = PlayerMapper.ToDomain(entity);
        var score = calculator.CalculateScore(domain.Matches, domain.IsDisqualified, domain.PenaltyPoints);
        return new PlayerDto(entity.Id, entity.Name, score, entity.IsDisqualified, entity.PenaltyPoints, entity.Matches.Count);
    }

    public static PlayerDetailDto ToDetailDto(PlayerEntity entity, ScoreCalculator calculator)
    {
        var dto = ToDto(entity, calculator);
        var matches = entity.Matches
            .OrderBy(m => m.Id)
            .Select(m => new MatchDto(m.Id, m.Result))
            .ToList();
        return new PlayerDetailDto(dto.Id, dto.Name, dto.Score, dto.IsDisqualified, dto.PenaltyPoints, matches);
    }

    public static ScoreBreakdownDto ToBreakdownDto(ScoreBreakdown breakdown)
    {
        var events = breakdown.Events.Select(ToEventDto).ToList();
        return new ScoreBreakdownDto(breakdown.FinalScore, breakdown.IsDisqualified, events);
    }

    private static ScoreEventDto ToEventDto(ScoreEvent scoreEvent) => scoreEvent switch
    {
        MatchScoredEvent e => new ScoreEventDto("match", e.Index, e.Outcome, null, e.Points, e.RunningScore),
        StreakBonusEvent e => new ScoreEventDto("streakBonus", null, null, e.AfterMatchIndex, e.Points, e.RunningScore),
        PenaltyEvent e => new ScoreEventDto("penalty", null, null, null, e.Points, e.RunningScore),
        ClampToZeroEvent e => new ScoreEventDto("clampToZero", null, null, null, null, e.RunningScore),
        DisqualificationEvent e => new ScoreEventDto("disqualification", null, null, null, null, e.RunningScore),
        _ => throw new InvalidOperationException($"Unknown score event: {scoreEvent.GetType().Name}.")
    };
}
