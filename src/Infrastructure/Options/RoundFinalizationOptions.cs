namespace BldLeague.Infrastructure.Options;

public class RoundFinalizationOptions
{
    public const string SectionName = "RoundFinalization";
    public string TimeZoneId { get; set; } = "Europe/Warsaw";
    public int Hour { get; set; } = 0;
    public int Minute { get; set; } = 1;
}
