namespace BldLeague.Web.Helpers;

public static class DateTimeExtensions
{
    public static string ToDisplayDate(this DateTime dateTime) => dateTime.ToString("dd.MM.yyyy");
}
