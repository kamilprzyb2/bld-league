using System.ComponentModel.DataAnnotations;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Create;

/// <summary>
/// Request to create a new league season linking a specific season to a specific league.
/// </summary>
public class CreateLeagueSeasonRequest : IRequest<CommandResult>
{
    public Guid SeasonId { get; set; }
    public Guid LeagueId { get; set; }
    [Range(0, int.MaxValue)] public int PromotionCount { get; set; }
    [Range(0, int.MaxValue)] public int RelegationCount { get; set; }
}
