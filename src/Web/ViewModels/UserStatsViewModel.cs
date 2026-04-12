using BldLeague.Domain.ValueObjects;

namespace BldLeague.Web.ViewModels;

public record UserStatsViewModel(
    int ValidSolves,
    int NonDnsSolves,
    SolveResult? AverageSingle,
    int Wins,
    int Losses,
    int Draws,
    int LongestSuccessStreak,
    int LongestWinStreak
);
