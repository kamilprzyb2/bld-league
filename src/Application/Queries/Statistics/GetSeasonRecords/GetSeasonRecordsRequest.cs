using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetSeasonRecords;

public record GetSeasonRecordsRequest : IRequest<IReadOnlyList<SeasonRecordDto>>;
