using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetActiveRound;

public class GetActiveRoundRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetActiveRoundRequest, ActiveRoundDto?>
{
    public async Task<ActiveRoundDto?> Handle(GetActiveRoundRequest request, CancellationToken cancellationToken)
    {
        var localToday = roundClock.LocalToday();
        var rounds = await unitOfWork.RoundRepository.GetRoundsActiveOnDateAsync(localToday);

        var round = rounds
            .Where(r => roundClock.IsRoundActive(r.StartDate, r.EndDate))
            .OrderByDescending(r => r.SeasonNumber)
            .ThenByDescending(r => r.RoundNumber)
            .FirstOrDefault();

        if (round == null)
            return null;

        return new ActiveRoundDto
        {
            RoundNumber = round.RoundNumber,
            EndsAtUtc = roundClock.LocalDayEndToUtc(round.EndDate),
        };
    }
}
