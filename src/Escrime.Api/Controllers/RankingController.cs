using Escrime.Api.Data;
using Escrime.Api.Dtos;
using Escrime.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Escrime.Api.Controllers;

[ApiController]
[Route("api/ranking")]
public class RankingController : ControllerBase
{
    private readonly EscrimeDbContext _db;
    private readonly ScoreCalculator _calculator;

    public RankingController(EscrimeDbContext db, ScoreCalculator calculator)
    {
        _db = db;
        _calculator = calculator;
    }

    /// <summary>Classement complet par score décroissant (tri stable : les ex æquo gardent leur ordre d'inscription).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RankingEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RankingEntryDto>>> GetRanking()
    {
        return await ComputeRanking();
    }

    /// <summary>Le champion du tournoi (premier du classement).</summary>
    [HttpGet("champion")]
    [ProducesResponseType(typeof(RankingEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RankingEntryDto>> GetChampion()
    {
        var ranking = await ComputeRanking();
        if (ranking.Count == 0)
            return NotFound();

        return ranking[0];
    }

    private async Task<List<RankingEntryDto>> ComputeRanking()
    {
        var players = await _db.Players
            .Include(p => p.Matches.OrderBy(m => m.Id))
            .AsNoTracking()
            .ToListAsync();

        // Même règle que TournamentRanking.GetRanking : tri stable décroissant
        // sur le score calculé par le domaine (jamais stocké).
        return players
            .Select(p => (Entity: p, Score: _calculator.CalculateScore(
                PlayerMapper.ToDomain(p).Matches, p.IsDisqualified, p.PenaltyPoints)))
            .OrderByDescending(x => x.Score)
            .Select((x, index) => new RankingEntryDto(index + 1, x.Entity.Id, x.Entity.Name, x.Score))
            .ToList();
    }
}
