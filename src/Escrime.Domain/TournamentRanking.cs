namespace Escrime.Domain;

public class TournamentRanking
{
    private readonly ScoreCalculator _scoreCalculator;

    public TournamentRanking(ScoreCalculator scoreCalculator)
    {
        _scoreCalculator = scoreCalculator;
    }

    /// <summary>
    /// Classe les joueurs par score décroissant.
    /// </summary>
    public List<Player> GetRanking(List<Player> players)
    {
        // OrderByDescending est un tri stable : les ex æquo gardent leur ordre d'entrée (H2)
        return players
            .OrderByDescending(p => _scoreCalculator.CalculateScore(p.Matches, p.IsDisqualified, p.PenaltyPoints))
            .ToList();
    }

    /// <summary>
    /// Trouve le champion (joueur avec le meilleur score), null si aucun joueur.
    /// </summary>
    public Player? GetChampion(List<Player> players)
    {
        throw new NotImplementedException();
    }
}
