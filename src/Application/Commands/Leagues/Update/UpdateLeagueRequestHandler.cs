using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Leagues.Update;

/// <summary>
/// Handles updating a league's identifier, rejecting the request if the league is not found or the new identifier conflicts.
/// </summary>
public class UpdateLeagueRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLeagueRequest, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateLeagueRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.LeagueRepository;

        if (!await repository.ExistsAsync(l => l.Id == request.LeagueId))
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono ligi z id: {request.LeagueId}.");
        }

        if (await repository.ExistsAsync(l => l.LeagueIdentifier == request.LeagueIdentifier && l.Id != request.LeagueId))
        {
            return CommandResult.Fail(
                nameof(request.LeagueIdentifier),
                "Inna liga z takim identyfikatorem już istnieje.");
        }

        var league = new League
        {
            Id = request.LeagueId,
            LeagueIdentifier = request.LeagueIdentifier,
        };

        repository.Update(league);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Zapisano dane ligi");
    }
}
