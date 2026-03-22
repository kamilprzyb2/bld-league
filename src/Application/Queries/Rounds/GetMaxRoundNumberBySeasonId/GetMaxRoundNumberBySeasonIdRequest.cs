using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetMaxRoundNumberBySeasonId;

/// <summary>
/// Request to retrieve the highest round number across all seasons (used to determine the latest round).
/// </summary>
public class GetMaxRoundNumberBySeasonIdRequest(Guid seasonId) : IRequest<int?>
{
    public Guid SeasonId { get; set; } = seasonId;
}
