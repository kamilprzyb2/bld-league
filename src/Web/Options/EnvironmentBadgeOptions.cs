using System.Text.RegularExpressions;

namespace BldLeague.Web.Options;

public partial class EnvironmentBadgeOptions
{
    private string? _color;

    public string? Name { get; set; }

    public string? Color
    {
        get => _color;
        set => _color = value is not null && HexColorRegex().IsMatch(value) ? value : null;
    }

    [GeneratedRegex("^#[0-9a-fA-F]{3}(?:[0-9a-fA-F]{3})?$")]
    private static partial Regex HexColorRegex();
}
