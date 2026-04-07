using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Commands.LeagueSeasonStandings.Refresh;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonStandings.RefreshAll;

/// <summary>
/// Handles recalculating cumulative standings for every league season in the database.
/// </summary>
public class RefreshAllLeagueSeasonStandingsRequestHandler(IUnitOfWork unitOfWork, ISender sender)
    : IRequestHandler<RefreshAllLeagueSeasonStandingsRequest, CommandResult>
{
    public async Task<CommandResult> Handle(RefreshAllLeagueSeasonStandingsRequest request, CancellationToken cancellationToken)
    {
        var leagueSeasons = await unitOfWork.LeagueSeasonRepository.GetAllProjectedAsync();

        foreach (var ls in leagueSeasons)
        {
            var result = await sender.Send(new RefreshLeagueSeasonStandingsRequest(ls.LeagueSeasonId), cancellationToken);
            if (!result.Success)
                return CommandResult.FailGeneral($"Błąd przy przeliczaniu liga-sezonu {ls.LeagueName} (sezon {ls.SeasonNumber}): {result.Message}");
        }

        return CommandResult.Ok($"Zaktualizowano klasyfikacje wszystkich liga-sezonów ({leagueSeasons.Count}).");
    }
}
