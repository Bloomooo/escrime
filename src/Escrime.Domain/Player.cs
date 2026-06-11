namespace Escrime.Domain;

public class Player
{
    public string Name { get; set; } = "";
    public List<MatchResult> Matches { get; set; } = new();
    public bool IsDisqualified { get; set; }
    public int PenaltyPoints { get; set; }
}
