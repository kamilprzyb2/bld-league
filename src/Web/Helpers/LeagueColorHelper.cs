namespace BldLeague.Web.Helpers;

/// <summary>
/// Maps a league identifier to a stable Bootstrap background utility class. Used for the
/// league marker tile on the home page and the /Matches/Recent page.
///
/// The mapping is deterministic across runs (no <c>string.GetHashCode()</c>, which is not
/// stable across runtimes) — same identifier always maps to the same color.
/// </summary>
public static class LeagueColorHelper
{
    private static readonly string[] Palette =
    [
        "bg-primary",
        "bg-success",
        "bg-danger",
        "bg-warning",
        "bg-info",
        "bg-dark",
    ];

    public static string GetBadgeBg(string? leagueIdentifier)
    {
        if (string.IsNullOrEmpty(leagueIdentifier))
            return "bg-secondary";

        // FNV-1a 32-bit hash over UTF-16 code units — stable across runtimes.
        const uint fnvOffset = 2166136261;
        const uint fnvPrime = 16777619;
        var hash = fnvOffset;
        foreach (var c in leagueIdentifier)
        {
            hash ^= c;
            hash *= fnvPrime;
        }

        var index = (int)(hash % (uint)Palette.Length);
        return Palette[index];
    }
}
