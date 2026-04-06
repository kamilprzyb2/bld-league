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
        var allRounds = await unitOfWork.RoundRepository.GetAllRoundSummariesAsync();
        var finishedRounds = allRounds.Where(r => r.EndDate < DateTime.UtcNow).ToList();

        foreach (var round in finishedRounds)
        {
            var result = await sender.Send(new RefreshRoundStandingsRequest(round.Id), cancellationToken);
            if (!result.Success)
                return CommandResult.FailGeneral($"Błąd przy przeliczaniu kolejki {round.RoundNumber} (sezon {round.SeasonNumber}): {result.Message}");
        }

        return CommandResult.Ok($"Zaktualizowano klasyfikacje wszystkich zakończonych kolejek ({finishedRounds.Count}).");
    }
}
