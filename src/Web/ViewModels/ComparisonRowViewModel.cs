namespace BldLeague.Web.ViewModels;

public record ComparisonRowViewModel(
    string Label,
    string ValueA,
    string ValueB,
    bool HighlightA,
    bool HighlightB
);
