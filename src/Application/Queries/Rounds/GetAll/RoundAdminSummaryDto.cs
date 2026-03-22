namespace BldLeague.Application.Queries.Rounds.GetAll;

/// <summary>
/// Summary data transfer object for the admin round list, including season context, round number, and date range.
/// </summary>
public record RoundAdminSummaryDto(Guid Id, Guid SeasonId, int SeasonNumber, int RoundNumber, DateTime StartDate, DateTime EndDate)
{
    public string SeasonName => $"Sezon {SeasonNumber}";
    public string RoundName => $"Kolejka {RoundNumber}";
}
