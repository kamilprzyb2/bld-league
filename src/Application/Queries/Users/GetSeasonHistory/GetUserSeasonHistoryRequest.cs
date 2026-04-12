using MediatR;

namespace BldLeague.Application.Queries.Users.GetSeasonHistory;

public class GetUserSeasonHistoryRequest : IRequest<IReadOnlyCollection<UserSeasonHistoryDto>>
{
    public Guid UserId { get; set; }
}
