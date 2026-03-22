using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.Delete;

/// <summary>
/// Handles removing a user from a league season roster, returning a failure result if the assignment does not exist.
/// </summary>
public class DeleteLeagueSeasonUserRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLeagueSeasonUserRequest, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteLeagueSeasonUserRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.LeagueSeasonUserRepository;
        var leagueSeasonUser = await repository.GetAsync(request.LeagueSeasonId, request.UserId);

        if (leagueSeasonUser == null)
        {
            return CommandResult.FailGeneral($"Nie znaleziono użytkownika {request.UserId} w ligo-sezonie {request.LeagueSeasonId}.");
        }

        repository.Delete(leagueSeasonUser);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Usunięto użytkownika z ligo-sezonu.");
    }
}
