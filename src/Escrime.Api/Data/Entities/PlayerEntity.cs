namespace Escrime.Api.Data.Entities;

public class PlayerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsDisqualified { get; set; }
    public int PenaltyPoints { get; set; }
    public List<MatchResultEntity> Matches { get; set; } = new();
}
