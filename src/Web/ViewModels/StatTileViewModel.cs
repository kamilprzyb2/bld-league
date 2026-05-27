namespace BldLeague.Web.ViewModels;

/// <summary>
/// Single tile in the global statistics surface — used on the home page and Statistics page.
/// </summary>
public record StatTileViewModel(
    string IconClass,
    string IconColorClass,
    string PrimaryText,
    string Subtitle);
