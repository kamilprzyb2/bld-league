using BldLeague.Application.Queries.Matches.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetAll;

/// <summary>
/// Request to retrieve all matches as a collection of admin summary DTOs.
/// </summary>
public class GetAllMatchesRequest : IRequest<IReadOnlyCollection<MatchAdminSummaryDto>>;
