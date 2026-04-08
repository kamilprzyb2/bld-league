using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetActiveFormUrl;

/// <summary>
/// Handles retrieving the submission form URL for the currently active round.
/// </summary>
public class GetActiveRoundFormUrlRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetActiveRoundFormUrlRequest, ActiveRoundFormDto?>
{
    public async Task<ActiveRoundFormDto?> Handle(GetActiveRoundFormUrlRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.RoundRepository.GetActiveRoundFormUrlAsync(DateTime.UtcNow);
    }
}
