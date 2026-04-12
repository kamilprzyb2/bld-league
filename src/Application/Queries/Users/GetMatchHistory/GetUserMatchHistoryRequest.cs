using MediatR;

namespace BldLeague.Application.Queries.Users.GetMatchHistory;

public class GetUserMatchHistoryRequest : IRequest<IReadOnlyCollection<UserMatchHistoryDto>>
{
    public Guid UserId { get; set; }
}
