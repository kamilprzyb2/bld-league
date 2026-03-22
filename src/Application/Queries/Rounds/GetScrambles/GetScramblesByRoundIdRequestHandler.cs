using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Rounds.GetScrambles;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetScrambles;

/// <summary>
/// Handles retrieving scramble notations for a round, projected into DTOs ordered by scramble number.
/// </summary>
public class GetScramblesByRoundIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetScramblesByRoundIdRequest, IReadOnlyCollection<ScrambleDto>>
{
    public async Task<IReadOnlyCollection<ScrambleDto>> Handle(GetScramblesByRoundIdRequest request, CancellationToken cancellationToken)
    {
        var scrambles = await unitOfWork.ScrambleRepository.GetByRoundIdAsync(request.RoundId);
        return scrambles
            .Select(s => new ScrambleDto { ScrambleNumber = s.ScrambleNumber, Notation = s.Notation })
            .ToList();
    }
}
