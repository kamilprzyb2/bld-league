namespace BldLeague.Web.ViewModels;

/// <summary>
/// Wraps a live-view row with the variant styling for the two no-reveal sections
/// ("Wgrane, oczekuje na przeciwnika" and "Brak wyniku") so the row markup is rendered
/// from a single shared partial.
/// </summary>
public class LiveRoundIdentityRowViewModel
{
    public required ActiveRoundLiveRowViewModel Row { get; set; }
    public required string BadgeCssClass { get; set; }
    public required string IconCssClass { get; set; }
    public required string BadgeText { get; set; }
}
