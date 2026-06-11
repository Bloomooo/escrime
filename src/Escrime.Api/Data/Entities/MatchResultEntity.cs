using Escrime.Domain;

namespace Escrime.Api.Data.Entities;

public class MatchResultEntity
{
    // L'Id auto-incrémenté sert d'ordre chronologique : les combats sont
    // toujours relus triés par Id (le bonus de série dépend de l'ordre).
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public MatchResult.Result Result { get; set; }
}
