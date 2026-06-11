using Escrime.Domain;

namespace Escrime.Api.Services;

/// <summary>
/// Implémentation de notification par log (un vrai canal — email, push —
/// remplacerait cette classe sans toucher au domaine).
/// </summary>
public class LoggingNotificationService : INotificationService
{
    private readonly ILogger<LoggingNotificationService> _logger;

    public LoggingNotificationService(ILogger<LoggingNotificationService> logger)
    {
        _logger = logger;
    }

    public void NotifyChampion(string playerName, int score)
    {
        _logger.LogInformation("🏆 Champion du tournoi : {PlayerName} avec {Score} points !", playerName, score);
    }
}
