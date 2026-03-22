using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Commands.RoundStandings.Refresh;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Create;

/// <summary>
/// Handles creating a new match, validating players and round membership, processing solves, and refreshing round standings.
/// </summary>
public class CreateMatchRequestHandler(IUnitOfWork unitOfWork, ISender sender)
    : IRequestHandler<CreateMatchRequest, CommandResult>
{
    public async Task<CommandResult> Handle(CreateMatchRequest request, CancellationToken cancellationToken)
    {
        var leagueSeason = await unitOfWork.LeagueSeasonRepository
            .GetByIdAsync(request.LeagueSeasonId);

        if (leagueSeason == null)
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono ligo-sezonu z ID: {request.LeagueSeasonId}.");
        }

        if (!await unitOfWork.RoundRepository.ExistsAsync(request.RoundId))
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono kolejki z ID: {request.RoundId}.");
        }

        if (!await unitOfWork.LeagueSeasonUserRepository
                .ExistsAsync(u => u.UserId == request.UserAId &&
                                  u.LeagueSeasonId == request.LeagueSeasonId))
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono użytkownika z ID: {request.UserAId} w ligo-sezonie {request.LeagueSeasonId}.");
        }

        if (await unitOfWork.MatchRepository.ExistsAsync(
            m=>m.LeagueSeasonId == request.LeagueSeasonId &&
               m.RoundId == request.RoundId &&
               (m.UserAId == request.UserAId || m.UserBId == request.UserAId)))
        {
            return CommandResult.Fail(
                nameof(request.UserAId),
                "Ten użytkownik ma już przypisany mecz w tej kolejce ligo-sezonu."
            );
        }

        if (request.UserBId != Guid.Empty)
        {
            if (!await unitOfWork.LeagueSeasonUserRepository
                    .ExistsAsync(u => u.UserId == request.UserBId &&
                                      u.LeagueSeasonId == request.LeagueSeasonId))
            {
                return CommandResult.FailGeneral(
                    $"Nie znaleziono użytkownika z ID: {request.UserAId} w ligo-sezonie {request.LeagueSeasonId}.");
            }

            if (await unitOfWork.MatchRepository.ExistsAsync(
                    m=>m.LeagueSeasonId == request.LeagueSeasonId &&
                       m.RoundId == request.RoundId &&
                       (m.UserAId == request.UserBId || m.UserBId == request.UserBId)))
            {
                return CommandResult.Fail(
                    nameof(request.UserBId),
                    "Ten użytkownik ma już przypisany mecz w tej kolejce ligo-sezonu."
                );
            }
        }

        if (request.UserASolves.Count != Match.SOLVES_PER_MATCH || request.UserBSolves.Count != Match.SOLVES_PER_MATCH)
        {
            return CommandResult.FailGeneral(
                $"Podano nieprawidłową liczba wyników: {request.UserASolves.Count} i {request.UserBSolves.Count}" +
                $"\nOczekiwana wartość to {Match.SOLVES_PER_MATCH}");
        }

        // Start Transaction
        await unitOfWork.BeginTransactionAsync();

        // Create match
        var match = Match.Create(
            request.LeagueSeasonId, request.RoundId, request.UserAId,
            request.UserBId == Guid.Empty ? null : request.UserBId, request.UserAScore, request.UserBScore);

        // Add solves, calculate scores
        await MatchSolvesProcessor.ProcessAsync(unitOfWork, match, request.UserASolves, request.UserBSolves);

        // Add match
        await unitOfWork.MatchRepository.AddAsync(match);

        // Save
        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        await sender.Send(new RefreshRoundStandingsRequest(request.RoundId), cancellationToken);

        return CommandResult.Ok("Mecz został dodany.");
    }
}
