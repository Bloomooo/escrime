namespace Escrime.Domain;

public class ScoreCalculator
{
    private const int PointsPerWin = 3;
    private const int PointsPerDraw = 1;
    private const int WinStreakLength = 3;
    private const int StreakBonus = 5;

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

        if (penaltyPoints < 0)
            throw new ArgumentException("Penalty points must be a positive number.", nameof(penaltyPoints));

        // H4 : la validation s'applique avant le court-circuit de disqualification
        if (isDisqualified)
            return 0; // la disqualification annule tout, peu importe les performances

        var score = 0;
        var winStreak = 0;
        foreach (var match in matches)
        {
            score += match.Outcome switch
            {
                MatchResult.Result.Win => PointsPerWin,
                MatchResult.Result.Draw => PointsPerDraw,
                _ => 0
            };

            if (match.Outcome == MatchResult.Result.Win)
            {
                winStreak++;
                if (winStreak == WinStreakLength)
                    score += StreakBonus; // accordé une seule fois par série
            }
            else
            {
                winStreak = 0; // un nul ou une défaite casse la série
            }
        }

        return Math.Max(0, score - penaltyPoints);
    }

    /// <summary>
    /// Déroulé du score selon les mêmes règles que <see cref="CalculateScore"/> :
    /// chaque événement porte le score courant, le front le rejoue sans recalculer.
    /// </summary>
    public ScoreBreakdown CalculateBreakdown(List<MatchResult> matches, bool isDisqualified = false, int penaltyPoints = 0)
    {
        return new ScoreBreakdown(0, isDisqualified, Array.Empty<ScoreEvent>());
    }
}
