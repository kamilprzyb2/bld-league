using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetRollingAo12Leaders;

public record GetRollingAo12LeadersRequest : IRequest<RollingAo12LeadersDto>;
