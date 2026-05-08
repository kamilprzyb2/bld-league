using MediatR;

namespace BldLeague.Application.Queries.Matches.GetActiveSubmission;

public record GetActiveSubmissionRequest(Guid UserId) : IRequest<ActiveSubmissionDto?>;
