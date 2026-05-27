using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetSubmissionHeatmap;

public record GetSubmissionHeatmapRequest : IRequest<SubmissionHeatmapDto>;
