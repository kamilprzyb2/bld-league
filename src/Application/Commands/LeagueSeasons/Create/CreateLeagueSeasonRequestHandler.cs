using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Create;

/// <summary>
/// Handles creating a new league season, validating that both the season and league exist and the combination is unique.
/// </summary>
public class CreateLeagueSeasonRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLeagueSeasonRequest, CommandResult>
{
    public async Task<CommandResult> Handle(CreateLeagueSeasonRequest request, CancellationToken cancellationToken)
    {
        var season = await unitOfWork.SeasonRepository.GetByIdAsync(request.SeasonId);
        if (season == null)
            return CommandResult.FailGeneral($"Nie znaleziono sezonu z ID: {request.SeasonId}.");

        var league = await unitOfWork.LeagueRepository.GetByIdAsync(request.LeagueId);
        if (league == null)
            return CommandResult.FailGeneral($"Nie znaleziono ligi z ID: {request.LeagueId}.");

        var exists = await unitOfWork.LeagueSeasonRepository.ExistsAsync(
            ls => ls.SeasonId == request.SeasonId && ls.LeagueId == request.LeagueId);
        if (exists)
            return CommandResult.FailGeneral("Ten liga-sezon już istnieje.");

        var leagueSeason = LeagueSeason.Create(league, season);
        await unitOfWork.LeagueSeasonRepository.AddAsync(leagueSeason);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok($"Utworzono liga-sezon: {league.LeagueName} — {season.SeasonName}.");
    }
}
