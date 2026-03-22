using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Matches.GetMatchExport;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetMatchExport;

/// <summary>
/// Handles retrieving match export rows, applying optional filters for use in CSV download.
/// </summary>
public class GetMatchExportRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMatchExportRequest, IReadOnlyCollection<MatchExportRowDto>>
{
    public async Task<IReadOnlyCollection<MatchExportRowDto>> Handle(GetMatchExportRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.MatchRepository.GetMatchExportRowsAsync(request.SeasonNumber, request.LeagueIdentifier, request.RoundNumber);
    }
}
