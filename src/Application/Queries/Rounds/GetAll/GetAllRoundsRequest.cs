using BldLeague.Application.Queries.Rounds.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetAll;

/// <summary>
/// Request to retrieve all rounds as a collection of admin summary DTOs.
/// </summary>
public class GetAllRoundsRequest : IRequest<IReadOnlyCollection<RoundAdminSummaryDto>>;
