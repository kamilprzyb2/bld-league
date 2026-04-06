using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Commands.RoundStandings.Refresh;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.RoundStandings.RefreshAll;

/// <summary>
/// Handles recalculating standings for every round in the database.
/// </summary>
public class RefreshAllRoundStandingsRequestHandler(IUnitOfWork unitOfWork, ISender sender)
    : IRequestHandler<RefreshAllRoundStandingsRequest, CommandResult>
{
    public async Task<CommandResult> Handle(RefreshAllRoundStandingsRequest request, CancellationToken cancellationToken)
    {
        var rounds = await unitOfWork.RoundRepository.GetAllRoundSummariesAsync();

        foreach (var round in rounds)
        {
            var result = await sender.Send(new RefreshRoundStandingsRequest(round.Id), cancellationToken);
            if (!result.Success)
                return CommandResult.FailGeneral($"Błąd przy przeliczaniu kolejki {round.RoundNumber} (sezon {round.SeasonNumber}): {result.Message}");
        }

        return CommandResult.Ok($"Zaktualizowano klasyfikacje wszystkich kolejek ({rounds.Count}).");
    }
}
