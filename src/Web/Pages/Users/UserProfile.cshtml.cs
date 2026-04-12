using BldLeague.Application.Queries.PlayerRankings.GetByUserId;
using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Application.Queries.Users.GetById;
using BldLeague.Application.Queries.Users.GetMatchHistory;
using BldLeague.Application.Queries.Users.GetRoundResults;
using BldLeague.Application.Queries.Users.GetSeasonHistory;
using BldLeague.Application.Queries.Users.GetSolves;
using BldLeague.Domain.ValueObjects;
using BldLeague.Web.Helpers;
using BldLeague.Web.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Users;

public class UserProfile(IMediator mediator) : PageModel
{
    public UserSummaryDto? Profile { get; set; }
    public PlayerRankingDto? Ranking { get; set; }
    public IReadOnlyCollection<UserRoundResultDto> RoundResults { get; set; } = [];
    public IReadOnlyCollection<UserMatchHistoryDto> MatchHistory { get; set; } = [];
    public IReadOnlyCollection<UserSeasonHistoryDto> SeasonHistory { get; set; } = [];
    public UserStatsViewModel Stats { get; set; } = new(0, 0, null, 0, 0, 0, 0, 0);

    public async Task<IActionResult> OnGet(Guid id)
    {
        Profile = await mediator.Send(new GetUserByIdRequest { UserId = id });
        if (Profile == null)
            return NotFound();

        Ranking = await mediator.Send(new GetPlayerRankingByUserIdRequest { UserId = id });
        RoundResults = await mediator.Send(new GetUserRoundResultsRequest { UserId = id });
        MatchHistory = await mediator.Send(new GetUserMatchHistoryRequest { UserId = id });
        SeasonHistory = await mediator.Send(new GetUserSeasonHistoryRequest { UserId = id });
        var solves = await mediator.Send(new GetUserSolvesRequest { UserId = id });

        Stats = ComputeStats(solves, MatchHistory);

        return Page();
    }

    private static UserStatsViewModel ComputeStats(
        IReadOnlyCollection<SolveResult> solves,
        IReadOnlyCollection<UserMatchHistoryDto> matchHistory)
    {
        var nonDnsSolves = solves.Where(s => !s.IsDns).ToList();
        var validSolves = nonDnsSolves.Where(s => s.IsValid).ToList();

        SolveResult? averageSingle = validSolves.Count > 0
            ? SolveResult.FromCentiseconds((int)Math.Round(validSolves.Average(s => (double)s.Centiseconds)))
            : null;

        int longestSuccessStreak = 0, currentSuccessStreak = 0;
        foreach (var s in solves)
        {
            if (s.IsDns) continue;
            if (s.IsValid) { currentSuccessStreak++; longestSuccessStreak = Math.Max(longestSuccessStreak, currentSuccessStreak); }
            else currentSuccessStreak = 0;
        }

        // Matches vs real opponents (exclude BYE), in chronological order
        var vsOpponent = matchHistory.Where(m => m.OpponentFullName != null).Reverse().ToList();
        int wins = vsOpponent.Count(m => m.ProfileUserScore > m.OpponentScore);
        int losses = vsOpponent.Count(m => m.ProfileUserScore < m.OpponentScore);
        int draws = vsOpponent.Count(m => m.ProfileUserScore == m.OpponentScore);

        int longestWinStreak = 0, currentWinStreak = 0;
        foreach (var m in vsOpponent)
        {
            if (m.ProfileUserScore > m.OpponentScore) { currentWinStreak++; longestWinStreak = Math.Max(longestWinStreak, currentWinStreak); }
            else currentWinStreak = 0;
        }

        return new UserStatsViewModel(validSolves.Count, nonDnsSolves.Count, averageSingle, wins, losses, draws, longestSuccessStreak, longestWinStreak);
    }

    public static string FormatSolveWithParens(UserRoundResultDto result, int index)
    {
        var solves = new[] { result.Solve1, result.Solve2, result.Solve3, result.Solve4, result.Solve5 };
        return SolveFormatHelper.FormatWithParens(solves, index);
    }
}
