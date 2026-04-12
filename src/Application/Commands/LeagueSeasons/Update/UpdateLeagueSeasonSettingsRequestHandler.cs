using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Update;

public class UpdateLeagueSeasonSettingsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLeagueSeasonSettingsRequest, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateLeagueSeasonSettingsRequest request, CancellationToken cancellationToken)
    {
        var leagueSeason = await unitOfWork.LeagueSeasonRepository.GetByIdAsync(request.LeagueSeasonId);
        if (leagueSeason == null)
            return CommandResult.FailGeneral($"Nie znaleziono liga-sezonu z ID: {request.LeagueSeasonId}.");

        leagueSeason.PromotionCount = request.PromotionCount;
        leagueSeason.RelegationCount = request.RelegationCount;
        leagueSeason.PlayoffPromotionCount = request.PlayoffPromotionCount;
        leagueSeason.PlayoffRelegationCount = request.PlayoffRelegationCount;
        unitOfWork.LeagueSeasonRepository.Update(leagueSeason);
        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Zaktualizowano ustawienia liga-sezonu.");
    }
}
