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
}
