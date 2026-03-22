using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.Create;

/// <summary>
/// Handles adding multiple users to a league season, validating existence and prior assignment before saving.
/// </summary>
public class CreateLeagueSeasonUsersRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLeagueSeasonUsersRequest, CommandResult>
{
    public async Task<CommandResult> Handle(CreateLeagueSeasonUsersRequest request, CancellationToken cancellationToken)
    {
        // Check if league season ID is valid.
        if (!await unitOfWork.LeagueSeasonRepository.ExistsAsync(request.LeagueSeasonId))
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono ligo-sezonu z ID: {request.LeagueSeasonId}"
            );
        }

        // Check if user IDs are valid.
        var allUsers = await unitOfWork.UserRepository.GetAllAsync();
        var missingUsers = request.UserIds.Except(allUsers.Select(u => u.Id)).ToList();
        if (missingUsers.Any())
        {
            return CommandResult.FailGeneral(
                $"Lista użytkowników zawiera nieprawidłowe ID: {string.Join(", ", missingUsers)}"
                );
        }

        // Check if user IDs are unassigned.
        var unassignedUsers =
            await unitOfWork.UserRepository.GetUnassignedUsersBySeasonIdAsync(request.LeagueSeasonId);

        var assignedUsers = request.UserIds.Except(unassignedUsers.Select(u => u.Id)).ToList();
        if (missingUsers.Any())
        {
            return CommandResult.FailGeneral(
                $"Użytkownicy o podanych ID: {string.Join(", ", assignedUsers)} są już przypisani do ligo-sezonu"
            );
        }

        List<LeagueSeasonUser> leagueSeasonUsers = [];
        foreach (var userId in request.UserIds)
        {
            leagueSeasonUsers.Add(
                LeagueSeasonUser.Create(request.LeagueSeasonId, userId));
        }

        await unitOfWork.LeagueSeasonUserRepository.AddRangeAsync(leagueSeasonUsers);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Dodano użytkowników do ligo-sezonu.");
    }
}
