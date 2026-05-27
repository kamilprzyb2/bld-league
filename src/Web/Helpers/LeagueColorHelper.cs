namespace BldLeague.Web.Helpers;

public static class LeagueColorHelper
{
    private const string FallbackBg = "var(--bs-secondary)";
    private const string FallbackText = "text-white";

    private static readonly (string Bg, string TextClass)[] Palette =
    [
        ("var(--bs-primary)", "text-white"),
        ("var(--bs-success)", "text-white"),
        ("var(--bs-danger)", "text-white"),
        ("var(--bs-warning)", "text-dark"),
        ("#ed8257", "text-dark"),
        ("var(--bs-purple)", "text-white"),
    ];

    public static (string Background, string TextClass) GetTileColors(string? leagueIdentifier)
    {
        if (string.IsNullOrEmpty(leagueIdentifier))
            return (FallbackBg, FallbackText);

        var first = char.ToUpperInvariant(leagueIdentifier[0]);
        if (first < 'A' || first > 'Z')
            return (FallbackBg, FallbackText);

        return Palette[(first - 'A') % Palette.Length];
    }
}
