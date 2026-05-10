namespace BldLeague.Application.Common;

/// <summary>
/// Resolves round timing semantics in the configured league time zone (default Europe/Warsaw).
/// Round StartDate/EndDate are bound from a date picker (time component is always 00:00),
/// so only the calendar date matters: a round is active for the full extent of its end day
/// in local time, regardless of UTC offset or DST.
/// </summary>
public class RoundClock(TimeZoneInfo timeZone)
{
    public DateTime LocalToday()
    {
        var localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
        return DateTime.SpecifyKind(localDate, DateTimeKind.Utc);
    }

    public bool IsRoundFinished(DateTime endDate)
        => LocalToday() > endDate.Date;

    public bool IsRoundActive(DateTime startDate, DateTime endDate)
    {
        var today = LocalToday();
        return today >= startDate.Date && today <= endDate.Date;
    }

    public DateTime ToLocal(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), timeZone);

    public DateTime LocalDayEndToUtc(DateTime localEndDate)
        => TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(localEndDate.Date.AddDays(1), DateTimeKind.Unspecified),
            timeZone);
}
