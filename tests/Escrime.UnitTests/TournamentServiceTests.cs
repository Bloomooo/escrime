using Escrime.Domain;
using FluentAssertions;
using Moq;

namespace Escrime.UnitTests;

public class TournamentServiceTests
{
    private readonly Mock<IPlayerRepository> _repositoryMock = new();
    private readonly Mock<INotificationService> _notifierMock = new();
    private readonly TournamentService _service;

    public TournamentServiceTests()
    {
        _service = new TournamentService(_repositoryMock.Object, _notifierMock.Object);
    }

    // TC-022
    [Fact]
    [Trait("Requirement", "REQ-E-014")]
    public void AnnounceChampion_WithPlayers_NotifiesChampionExactlyOnce()
    {
        // Arrange : le repository (mocké) retourne 2 joueurs, Aria est championne (14 pts)
        var aria = MakePlayer("Aria", "Win", "Win", "Win");      // 14 points
        var bran = MakePlayer("Bran", "Win", "Draw");            // 4 points
        _repositoryMock.Setup(r => r.GetAll()).Returns([aria, bran]);

        // Act
        var champion = _service.AnnounceChampion();

        // Assert
        champion.Should().BeSameAs(aria);
        _notifierMock.Verify(n => n.NotifyChampion("Aria", 14), Times.Once);
        _notifierMock.VerifyNoOtherCalls();
    }

    // TC-023
    [Fact]
    [Trait("Requirement", "REQ-E-014")]
    public void AnnounceChampion_NoPlayers_DoesNotNotify()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAll()).Returns([]);

        // Act
        var champion = _service.AnnounceChampion();

        // Assert
        champion.Should().BeNull();
        _notifierMock.Verify(n => n.NotifyChampion(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    private static Player MakePlayer(string name, params string[] results) => new()
    {
        Name = name,
        Matches = results.Select(r => new MatchResult(Enum.Parse<MatchResult.Result>(r))).ToList()
    };
}
