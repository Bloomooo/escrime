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
        if (matches is null)
            throw new ArgumentNullException(nameof(matches), "The match list cannot be null.");

        var score = 0;
        var winStreak = 0;
        foreach (var match in matches)
        {
            score += match.Outcome switch
            {
                MatchResult.Result.Win => 3,
                MatchResult.Result.Draw => 1,
                _ => 0
            };

            if (match.Outcome == MatchResult.Result.Win)
            {
                winStreak++;
                if (winStreak == 3)
                    score += 5; // bonus de série, accordé une seule fois par série
            }
            else
            {
                winStreak = 0; // un nul ou une défaite casse la série
            }
        }

        return score;
    }
}
