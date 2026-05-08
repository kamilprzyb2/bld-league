using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Submit;

public class SubmitMatchSolvesRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitMatchSolvesRequest, CommandResult>
{
    public async Task<CommandResult> Handle(SubmitMatchSolvesRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return CommandResult.FailGeneral("Nie znaleziono użytkownika.");

        var utcNow = DateTime.UtcNow;
        var match = await unitOfWork.MatchRepository.GetActiveMatchForUserAsync(request.UserId, utcNow);
        if (match == null)
            return CommandResult.FailGeneral("Brak aktywnego meczu do wgrania wyników.");

        if (match.HasUserSubmitted(request.UserId))
            return CommandResult.FailGeneral("Wyniki dla tego meczu zostały już wgrane.");

        if (request.Solves.Count != Match.SOLVES_PER_MATCH)
            return CommandResult.FailGeneral(
                $"Podano nieprawidłową liczbę wyników: {request.Solves.Count}. Oczekiwana wartość to {Match.SOLVES_PER_MATCH}.");

        var solveResults = request.Solves
            .Select(s => SolveResult.FromString(s.Result))
            .ToList();

        await unitOfWork.BeginTransactionAsync();

        await MatchSolvesProcessor.ReplaceUserSolvesAsync(unitOfWork, match, request.UserId, solveResults);

        if (match.UserAId == request.UserId)
            match.UserASubmittedAt = utcNow;
        else
            match.UserBSubmittedAt = utcNow;

        unitOfWork.MatchRepository.Update(match);

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Wyniki zostały wgrane.");
    }
}
