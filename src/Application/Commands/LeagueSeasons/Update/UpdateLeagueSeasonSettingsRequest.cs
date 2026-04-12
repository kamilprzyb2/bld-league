using System.ComponentModel.DataAnnotations;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Update;

public class UpdateLeagueSeasonSettingsRequest : IRequest<CommandResult>
{
    public Guid LeagueSeasonId { get; set; }
    [Range(0, int.MaxValue)] public int PromotionCount { get; set; }
    [Range(0, int.MaxValue)] public int RelegationCount { get; set; }
    [Range(0, int.MaxValue)] public int PlayoffPromotionCount { get; set; }
    [Range(0, int.MaxValue)] public int PlayoffRelegationCount { get; set; }
}
