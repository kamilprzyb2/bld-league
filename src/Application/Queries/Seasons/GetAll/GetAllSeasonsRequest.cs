using BldLeague.Application.Queries.Seasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Seasons.GetAll;

/// <summary>
/// Request to retrieve all seasons as a collection of summary DTOs.
/// </summary>
public class GetAllSeasonsRequest : IRequest<IReadOnlyCollection<SeasonSummaryDto>>;
