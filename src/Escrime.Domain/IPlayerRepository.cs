namespace Escrime.Domain;

/// <summary>
/// Accès aux joueurs persistés (implémenté par la couche infrastructure).
/// </summary>
public interface IPlayerRepository
{
    List<Player> GetAll();
}
