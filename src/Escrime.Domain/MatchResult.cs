namespace Escrime.Domain;

/// <summary>
/// Résultat d'un combat d'escrime, dans l'ordre chronologique du tournoi.
/// </summary>
public class MatchResult
{
    public enum Result
    {
        Win,  // Victoire
        Draw, // Match nul
        Loss  // Défaite
    }

    public Result Outcome { get; set; }

    // Constructeur pour faciliter les tests
    public MatchResult(Result outcome)
    {
        Outcome = outcome;
    }

    // Constructeur par défaut
    public MatchResult() { }
}
