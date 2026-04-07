using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.SetGroup;

/// <summary>
/// Handles batch-updating the subleague group for all users in a league season.
/// </summary>
public class SetLeagueSeasonUserGroupsBatchRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SetLeagueSeasonUserGroupsBatchRequest, CommandResult>
{
    public async Task<CommandResult> Handle(SetLeagueSeasonUserGroupsBatchRequest request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();

        foreach (var entry in request.Entries)
        {
            var leagueSeasonUser = await unitOfWork.LeagueSeasonUserRepository.GetAsync(request.LeagueSeasonId, entry.UserId);
            if (leagueSeasonUser == null)
            {
                return CommandResult.FailGeneral($"Nie znaleziono uczestnika ligo-sezonu (userId: {entry.UserId}).");
            }

            leagueSeasonUser.SubleagueGroup = entry.SubleagueGroup;
            unitOfWork.LeagueSeasonUserRepository.Update(leagueSeasonUser);
        }

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Zaktualizowano grupy uczestników.");
    }
}
