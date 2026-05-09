using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetSolves;

public class GetUserSolvesRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetUserSolvesRequest, IReadOnlyCollection<SolveResult>>
{
    public async Task<IReadOnlyCollection<SolveResult>> Handle(GetUserSolvesRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.SolveRepository.GetFinishedSolvesByUserIdAsync(request.UserId, roundClock.LocalToday());
    }
}
