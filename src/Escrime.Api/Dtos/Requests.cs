using System.ComponentModel.DataAnnotations;
using Escrime.Domain;

namespace Escrime.Api.Dtos;

public record CreatePlayerRequest([Required(AllowEmptyStrings = false)] string Name);

public record RecordMatchRequest([Required] MatchResult.Result Result);

public record AddPenaltyRequest([Range(0, int.MaxValue)] int Points);
