using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Submit;

public class SubmitMatchSolvesRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<SubmitMatchSolvesRequest, CommandResult>
{
    public async Task<CommandResult> Handle(SubmitMatchSolvesRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return CommandResult.FailGeneral("Nie znaleziono użytkownika.");

        var match = await unitOfWork.MatchRepository.GetActiveMatchForUserAsync(request.UserId, roundClock.LocalToday());
        if (match == null)
            return CommandResult.FailGeneral("Brak aktywnego meczu do wgrania wyników.");

        if (match.HasUserSubmitted(request.UserId))
            return CommandResult.FailGeneral("Wyniki dla tego meczu zostały już wgrane.");

        if (match.Id != request.MatchId)
            return CommandResult.FailGeneral(
                "Aktywna kolejka zmieniła się od momentu otwarcia formularza. Odśwież stronę i wpisz wyniki ponownie.");

        if (request.Solves.Count != Match.SOLVES_PER_MATCH)
            return CommandResult.FailGeneral(
                $"Podano nieprawidłową liczbę wyników: {request.Solves.Count}. Oczekiwana wartość to {Match.SOLVES_PER_MATCH}.");

        var solveResults = request.Solves
            .Select(s => SolveResult.FromString(s.Result))
            .ToList();

        await unitOfWork.BeginTransactionAsync();

        await MatchSolvesProcessor.ReplaceUserSolvesAsync(unitOfWork, match, request.UserId, solveResults);

        if (match.UserAId == request.UserId)
            match.UserASubmittedAt = DateTime.UtcNow;
        else
            match.UserBSubmittedAt = DateTime.UtcNow;

        unitOfWork.MatchRepository.Update(match);

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Wyniki zostały wgrane.");
    }
}
