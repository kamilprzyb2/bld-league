using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Update;

public class UpdateLeagueSeasonSettingsRequest : IRequest<CommandResult>
{
    public Guid LeagueSeasonId { get; set; }
    public int PromotionCount { get; set; }
    public int RelegationCount { get; set; }
}
