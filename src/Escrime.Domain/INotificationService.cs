namespace Escrime.Domain;

/// <summary>
/// Notification des participants du tournoi.
/// </summary>
public interface INotificationService
{
    void NotifyChampion(string playerName, int score);
}
