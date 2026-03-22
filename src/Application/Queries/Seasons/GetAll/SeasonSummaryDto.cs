namespace BldLeague.Application.Queries.Seasons.GetAll;

/// <summary>
/// Summary data transfer object for a season, carrying its ID and number.
/// </summary>
public record SeasonSummaryDto(Guid Id, int SeasonNumber)
{
    public string SeasonName => $"Sezon {SeasonNumber}";
}
