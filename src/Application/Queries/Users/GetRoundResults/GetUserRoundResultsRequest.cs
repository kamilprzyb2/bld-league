using MediatR;

namespace BldLeague.Application.Queries.Users.GetRoundResults;

public class GetUserRoundResultsRequest : IRequest<IReadOnlyCollection<UserRoundResultDto>>
{
    public Guid UserId { get; set; }
}
