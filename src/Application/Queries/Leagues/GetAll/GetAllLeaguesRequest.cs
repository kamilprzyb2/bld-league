using BldLeague.Application.Queries.Leagues.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Leagues.GetAll;

/// <summary>
/// Request to retrieve all leagues as a collection of summary DTOs.
/// </summary>
public class GetAllLeaguesRequest : IRequest<IReadOnlyCollection<LeagueSummaryDto>>;
