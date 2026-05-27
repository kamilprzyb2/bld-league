using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;

public record GetLeagueRecordsAndAveragesRequest : IRequest<IReadOnlyList<LeagueRecordDto>>;
