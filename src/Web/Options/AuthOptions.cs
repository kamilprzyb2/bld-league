namespace BldLeague.Web.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";
    public int ClaimsRefreshMinutes { get; set; } = 10;
    public int CookieExpireDays { get; set; } = 14;
}
