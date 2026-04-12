using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetSolves;

public class GetUserSolvesRequest : IRequest<IReadOnlyCollection<SolveResult>>
{
    public Guid UserId { get; init; }
}
