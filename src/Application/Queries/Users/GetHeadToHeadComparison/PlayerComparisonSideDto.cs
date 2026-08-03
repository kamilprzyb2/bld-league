using BldLeague.Application.Common;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

public record PlayerComparisonSideDto(
    Guid UserId,
    string FullName,
    string WcaId,
    string? AvatarThumbnailUrl,
    SolveResult? BestSingle,
    int? SingleRank,
    SolveResult? BestAverage,
    int? AverageRank,
    UserStats Stats,
    IReadOnlyList<RecentFormEntryDto> RecentForm,
    RecentFormStatsDto RecentStats
);
