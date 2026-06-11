namespace Escrime.Domain;

public class ScoreCalculator
{
    /// <summary>
    /// Calcule le score final d'un joueur selon les règles du tournoi.
    /// </summary>
    /// <param name="matches">Liste des résultats de combat dans l'ordre chronologique</param>
    /// <param name="isDisqualified">True si le joueur est disqualifié</param>
    /// <param name="penaltyPoints">Points de pénalité (nombre positif)</param>
    /// <returns>Score final (jamais négatif)</returns>
    public int CalculateScore(List<MatchResult> matches, bool isDisqualified = false, int penaltyPoints = 0)
    {
        var score = 0;
        foreach (var match in matches)
        {
            score += match.Outcome switch
            {
                MatchResult.Result.Win => 3,
                MatchResult.Result.Draw => 1,
                _ => 0
            };
        }

        return score;
    }
}
