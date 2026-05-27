using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetRollingAo25Leaders;

public record GetRollingAo25LeadersRequest : IRequest<RollingAo25LeadersDto>;
