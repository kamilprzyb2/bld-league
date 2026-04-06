using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.SetGroup;

/// <summary>
/// Handles updating the subleague group for a user in a league season.
/// </summary>
public class SetLeagueSeasonUserGroupRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SetLeagueSeasonUserGroupRequest, CommandResult>
{
    public async Task<CommandResult> Handle(SetLeagueSeasonUserGroupRequest request, CancellationToken cancellationToken)
    {
        var leagueSeasonUser = await unitOfWork.LeagueSeasonUserRepository.GetAsync(request.LeagueSeasonId, request.UserId);
        if (leagueSeasonUser == null)
            return CommandResult.FailGeneral("Nie znaleziono uczestnika ligo-sezonu.");

        leagueSeasonUser.SubleagueGroup = request.SubleagueGroup;
        unitOfWork.LeagueSeasonUserRepository.Update(leagueSeasonUser);
        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Zaktualizowano grupę uczestnika.");
    }
}
