using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Commands.RoundStandings.Refresh;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Delete;

/// <summary>
/// Handles deleting a match and triggering a round standings refresh for the affected round.
/// </summary>
public class DeleteMatchRequestHandler(IUnitOfWork unitOfWork, ISender sender)
    : IRequestHandler<DeleteMatchRequest, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteMatchRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.MatchRepository;

        var match = await repository.GetByIdAsync(request.Id);
        if (match == null)
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono meczu z ID: {request.Id}");
        }

        var roundId = match.RoundId;
        repository.Delete(match);
        await unitOfWork.SaveAsync();

        await sender.Send(new RefreshRoundStandingsRequest(roundId), cancellationToken);

        return CommandResult.Ok("Usunięto mecz");
    }
}
