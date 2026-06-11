namespace Escrime.Domain;

/// <summary>
/// Service métier : désigne le champion du tournoi et le notifie.
/// Dépendances injectées pour permettre les tests en isolation (Moq).
/// </summary>
public class TournamentService
{
    private readonly IPlayerRepository _repository;
    private readonly INotificationService _notifier;
    private readonly TournamentRanking _ranking;
    private readonly ScoreCalculator _scoreCalculator;

    public TournamentService(IPlayerRepository repository, INotificationService notifier)
    {
        _repository = repository;
        _notifier = notifier;
        _scoreCalculator = new ScoreCalculator();
        _ranking = new TournamentRanking(_scoreCalculator);
    }

    /// <summary>
    /// Détermine le champion, le notifie, et le retourne (null si aucun joueur).
    /// </summary>
    public Player? AnnounceChampion()
    {
        throw new NotImplementedException();
    }
}
