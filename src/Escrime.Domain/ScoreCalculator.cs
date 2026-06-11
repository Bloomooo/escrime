namespace Escrime.Domain;

public class ScoreCalculator
{
    private const int PointsPerWin = 3;
    private const int PointsPerDraw = 1;
    private const int WinStreakLength = 3;
    private const int StreakBonus = 5;

    /// <summary>
    /// Calcule le score final d'un joueur selon les règles du tournoi.
    /// Délègue au déroulé : une seule implémentation des règles.
    /// </summary>
    /// <param name="matches">Liste des résultats de combat dans l'ordre chronologique</param>
    /// <param name="isDisqualified">True si le joueur est disqualifié</param>
    /// <param name="penaltyPoints">Points de pénalité (nombre positif)</param>
    /// <returns>Score final (jamais négatif)</returns>
    public int CalculateScore(List<MatchResult> matches, bool isDisqualified = false, int penaltyPoints = 0)
    {
        return CalculateBreakdown(matches, isDisqualified, penaltyPoints).FinalScore;
    }

    /// <summary>
    /// Déroulé du score, événement par événement : chaque événement porte le
    /// score courant, le front le rejoue tel quel sans recalculer les règles.
    /// </summary>
    public ScoreBreakdown CalculateBreakdown(List<MatchResult> matches, bool isDisqualified = false, int penaltyPoints = 0)
    {
        if (matches is null)
            throw new ArgumentNullException(nameof(matches), "The match list cannot be null.");

        if (penaltyPoints < 0)
            throw new ArgumentException("Penalty points must be a positive number.", nameof(penaltyPoints));

        var events = new List<ScoreEvent>();
        var score = 0;
        var winStreak = 0;
        for (var index = 0; index < matches.Count; index++)
        {
            var outcome = matches[index].Outcome;
            var points = outcome switch
            {
                MatchResult.Result.Win => PointsPerWin,
                MatchResult.Result.Draw => PointsPerDraw,
                _ => 0
            };
            score += points;
            events.Add(new MatchScoredEvent(index, outcome, points, score));

            if (outcome == MatchResult.Result.Win)
            {
                winStreak++;
                if (winStreak == WinStreakLength)
                {
                    score += StreakBonus; // accordé une seule fois par série
                    events.Add(new StreakBonusEvent(index, StreakBonus, score));
                }
            }
            else
            {
                winStreak = 0; // un nul ou une défaite casse la série
            }
        }

        if (penaltyPoints > 0)
        {
            score -= penaltyPoints;
            events.Add(new PenaltyEvent(-penaltyPoints, score));
            if (score < 0)
            {
                score = 0;
                events.Add(new ClampToZeroEvent(0));
            }
        }

        // H4 : la disqualification annule tout, mais le déroulé reste raconté
        if (isDisqualified)
        {
            score = 0;
            events.Add(new DisqualificationEvent(0));
        }

        return new ScoreBreakdown(score, isDisqualified, events);
    }
}
