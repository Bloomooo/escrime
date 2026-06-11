using Escrime.Domain;
using Microsoft.EntityFrameworkCore;

namespace Escrime.Api.Data;

/// <summary>
/// Implémentation EF Core du contrat du domaine (utilisé par TournamentService).
/// </summary>
public class EfPlayerRepository : IPlayerRepository
{
    private readonly EscrimeDbContext _db;

    public EfPlayerRepository(EscrimeDbContext db)
    {
        _db = db;
    }

    public List<Player> GetAll() =>
        _db.Players
            .Include(p => p.Matches.OrderBy(m => m.Id))
            .AsNoTracking()
            .ToList()
            .Select(PlayerMapper.ToDomain)
            .ToList();
}
