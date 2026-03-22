using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Commands.RoundStandings.Refresh;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Edit;

/// <summary>
/// Handles editing a match by replacing its solves, recalculating scores, and refreshing round standings.
/// </summary>
public class EditMatchRequestHandler(IUnitOfWork unitOfWork, ISender sender)
    : IRequestHandler<EditMatchRequest, CommandResult>
{
    public async Task<CommandResult> Handle(EditMatchRequest request, CancellationToken cancellationToken)
    {
        var match = await unitOfWork.MatchRepository.GetMatchWithSolvesAsync(request.MatchId);

        if (match == null)
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono meczu z ID: {request.MatchId}.");
        }

        if (request.UserASolves.Count != Match.SOLVES_PER_MATCH)
        {
            return CommandResult.FailGeneral(
                $"Podano nieprawidłową liczbę wyników zawodnika A: {request.UserASolves.Count}. " +
                $"Oczekiwana wartość to {Match.SOLVES_PER_MATCH}.");
        }

        if (match.UserBId != null && request.UserBSolves.Count != Match.SOLVES_PER_MATCH)
        {
            return CommandResult.FailGeneral(
                $"Podano nieprawidłową liczbę wyników zawodnika B: {request.UserBSolves.Count}. " +
                $"Oczekiwana wartość to {Match.SOLVES_PER_MATCH}.");
        }

        await unitOfWork.BeginTransactionAsync();

        // Delete existing solves
        foreach (var solve in match.Solves.ToList())
        {
            unitOfWork.SolveRepository.Delete(solve);
        }

        // Recreate solves and recalculate scores
        await MatchSolvesProcessor.ProcessAsync(unitOfWork, match, request.UserASolves, request.UserBSolves);

        unitOfWork.MatchRepository.Update(match);

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        await sender.Send(new RefreshRoundStandingsRequest(match.RoundId), cancellationToken);

        return CommandResult.Ok("Mecz został zaktualizowany.");
    }
}
