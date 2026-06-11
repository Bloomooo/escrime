using Escrime.Api.Data;
using Escrime.Api.Data.Entities;
using Escrime.Api.Dtos;
using Escrime.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Escrime.Api.Controllers;

[ApiController]
[Route("api/players")]
public class PlayersController : ControllerBase
{
    private readonly EscrimeDbContext _db;
    private readonly ScoreCalculator _calculator;

    public PlayersController(EscrimeDbContext db, ScoreCalculator calculator)
    {
        _db = db;
        _calculator = calculator;
    }

    /// <summary>Inscrit un joueur au tournoi.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlayerDto>> Create(CreatePlayerRequest request)
    {
        var entity = new PlayerEntity { Name = request.Name };
        _db.Players.Add(entity);
        await _db.SaveChangesAsync();

        var dto = DtoMapper.ToDto(entity, _calculator);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
    }

    /// <summary>Liste tous les joueurs avec leur score calculé.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PlayerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PlayerDto>>> GetAll()
    {
        var players = await _db.Players
            .Include(p => p.Matches.OrderBy(m => m.Id))
            .AsNoTracking()
            .ToListAsync();

        return players.Select(p => DtoMapper.ToDto(p, _calculator)).ToList();
    }

    /// <summary>Détail d'un joueur, combats inclus.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlayerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDetailDto>> GetById(int id)
    {
        var entity = await FindPlayer(id);
        if (entity is null)
            return NotFound();

        return DtoMapper.ToDetailDto(entity, _calculator);
    }

    /// <summary>Désinscrit un joueur (ses combats sont supprimés en cascade).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Players.FindAsync(id);
        if (entity is null)
            return NotFound();

        _db.Players.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Enregistre le résultat d'un combat (dans l'ordre chronologique).</summary>
    [HttpPost("{id:int}/matches")]
    [ProducesResponseType(typeof(MatchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchDto>> RecordMatch(int id, RecordMatchRequest request)
    {
        var entity = await FindPlayer(id);
        if (entity is null)
            return NotFound();

        var match = new MatchResultEntity { PlayerId = id, Result = request.Result };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id }, new MatchDto(match.Id, match.Result));
    }

    /// <summary>Inflige des points de pénalité (cumulés, le score final reste >= 0).</summary>
    [HttpPost("{id:int}/penalties")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDto>> AddPenalty(int id, AddPenaltyRequest request)
    {
        var entity = await FindPlayer(id);
        if (entity is null)
            return NotFound();

        entity.PenaltyPoints += request.Points;
        await _db.SaveChangesAsync();

        return DtoMapper.ToDto(entity, _calculator);
    }

    /// <summary>Disqualifie un joueur : son score devient 0 définitivement.</summary>
    [HttpPost("{id:int}/disqualification")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDto>> Disqualify(int id)
    {
        var entity = await FindPlayer(id);
        if (entity is null)
            return NotFound();

        entity.IsDisqualified = true;
        await _db.SaveChangesAsync();

        return DtoMapper.ToDto(entity, _calculator);
    }

    private Task<PlayerEntity?> FindPlayer(int id) =>
        _db.Players
            .Include(p => p.Matches.OrderBy(m => m.Id))
            .SingleOrDefaultAsync(p => p.Id == id);
}
