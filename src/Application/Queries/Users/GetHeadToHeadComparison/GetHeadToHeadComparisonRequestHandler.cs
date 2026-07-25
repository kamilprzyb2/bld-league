using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

public class GetHeadToHeadComparisonRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetHeadToHeadComparisonRequest, HeadToHeadComparisonDto?>
{
    private const int RECENT_FORM_MATCHES = 5;

    public async Task<HeadToHeadComparisonDto?> Handle(GetHeadToHeadComparisonRequest request, CancellationToken cancellationToken)
    {
        var userA = await unitOfWork.UserRepository.GetSummaryByIdAsync(request.UserAId);
        var userB = await unitOfWork.UserRepository.GetSummaryByIdAsync(request.UserBId);
        if (userA == null || userB == null)
            return null;

        var localToday = roundClock.LocalToday();

        var rankingA = await unitOfWork.PlayerRankingRepository.GetByUserIdAsync(request.UserAId);
        var rankingB = await unitOfWork.PlayerRankingRepository.GetByUserIdAsync(request.UserBId);
        var matchesA = await unitOfWork.MatchRepository.GetFinishedMatchesByUserIdAsync(request.UserAId, localToday);
        var matchesB = await unitOfWork.MatchRepository.GetFinishedMatchesByUserIdAsync(request.UserBId, localToday);
        var solvesA = await unitOfWork.SolveRepository.GetFinishedSolvesByUserIdAsync(request.UserAId, localToday);
        var solvesB = await unitOfWork.SolveRepository.GetFinishedSolvesByUserIdAsync(request.UserBId, localToday);
        var standingsA = FilterFinished(await unitOfWork.RoundStandingRepository.GetByUserIdWithDetailsAsync(request.UserAId), localToday);
        var standingsB = FilterFinished(await unitOfWork.RoundStandingRepository.GetByUserIdWithDetailsAsync(request.UserBId), localToday);

        // Matches are ordered newest first by the repository.
        var meetings = matchesA
            .Where(m => m.UserAId == request.UserBId || m.UserBId == request.UserBId)
            .Select(m =>
            {
                var perspective = MatchPerspective.For(m, request.UserAId);
                return new HeadToHeadMatchDto(
                    m.Id,
                    m.Round.Season.SeasonNumber,
                    m.Round.RoundNumber,
                    m.LeagueSeason.League.LeagueIdentifier,
                    perspective.SelfScore,
                    perspective.OpponentScore);
            })
            .ToList();

        int winsA = meetings.Count(m => m.ScoreA > m.ScoreB);
        int winsB = meetings.Count(m => m.ScoreA < m.ScoreB);
        int draws = meetings.Count(m => m.ScoreA == m.ScoreB);

        var meetingRoundKeys = meetings.Select(m => (m.SeasonNumber, m.RoundNumber)).ToHashSet();

        var commonRounds = standingsA
            .Join(standingsB,
                a => (a.Round.Season.SeasonNumber, a.Round.RoundNumber),
                b => (b.Round.Season.SeasonNumber, b.Round.RoundNumber),
                (a, b) => new CommonRoundDto(
                    a.Round.Season.SeasonNumber,
                    a.Round.RoundNumber,
                    a.League.LeagueIdentifier,
                    b.League.LeagueIdentifier,
                    a.Best,
                    a.Average,
                    b.Best,
                    b.Average,
                    meetingRoundKeys.Contains((a.Round.Season.SeasonNumber, a.Round.RoundNumber))))
            .OrderBy(c => c.SeasonNumber)
            .ThenBy(c => c.RoundNumber)
            .ToList();

        var upcomingMatch = await unitOfWork.MatchRepository.GetUnfinishedMatchBetweenUsersAsync(request.UserAId, request.UserBId, localToday);
        var upcomingMeeting = upcomingMatch == null
            ? null
            : new UpcomingMeetingDto(
                upcomingMatch.Round.Season.SeasonNumber,
                upcomingMatch.Round.RoundNumber,
                upcomingMatch.LeagueSeason.League.LeagueIdentifier,
                roundClock.IsRoundActive(upcomingMatch.Round.StartDate, upcomingMatch.Round.EndDate));

        return new HeadToHeadComparisonDto(
            await BuildSideAsync(userA, rankingA, matchesA, solvesA),
            await BuildSideAsync(userB, rankingB, matchesB, solvesB),
            winsA,
            winsB,
            draws,
            meetings.Sum(m => m.ScoreA),
            meetings.Sum(m => m.ScoreB),
            meetings,
            upcomingMeeting,
            commonRounds);
    }

    private static List<RoundStanding> FilterFinished(IReadOnlyCollection<RoundStanding> standings, DateTime localToday)
        => standings.Where(rs => rs.Round.EndDate < localToday).ToList();

    private async Task<PlayerComparisonSideDto> BuildSideAsync(
        UserSummaryDto user,
        PlayerRanking? ranking,
        IReadOnlyCollection<Match> matchesNewestFirst,
        IReadOnlyCollection<SolveResult> solves)
    {
        var orientedMatches = matchesNewestFirst
            .Select(m => (Match: m, Perspective: MatchPerspective.For(m, user.Id)))
            .ToList();

        var stats = UserStatsCalculator.Calculate(solves, orientedMatches.Select(m => m.Perspective).ToList());

        var recentMatches = orientedMatches
            .Where(m => m.Perspective.OpponentId != null)
            .Take(RECENT_FORM_MATCHES)
            .ToList();

        var recentForm = recentMatches
            .Select(m => new RecentFormEntryDto(
                m.Perspective.SelfScore > m.Perspective.OpponentScore ? MatchOutcome.Win
                    : m.Perspective.SelfScore < m.Perspective.OpponentScore ? MatchOutcome.Loss
                    : MatchOutcome.Draw,
                m.Match.Round.Season.SeasonNumber,
                m.Match.Round.RoundNumber,
                m.Perspective.SelfScore,
                m.Perspective.OpponentScore,
                m.Perspective.OpponentFullName!))
            .ToList();

        var recentSolves = await unitOfWork.SolveRepository.GetByUserAndMatchIdsAsync(
            user.Id, recentMatches.Select(m => m.Match.Id).ToList());
        var recentNonDnsSolves = recentSolves.Where(s => !s.IsDns).ToList();
        var recentValidSolves = recentNonDnsSolves.Where(s => s.IsValid).ToList();
        var recentStats = new RecentFormStatsDto(
            UserStatsCalculator.CalculateMean(recentValidSolves),
            recentValidSolves.Count,
            recentNonDnsSolves.Count);

        return new PlayerComparisonSideDto(
            user.Id,
            user.FullName,
            user.WcaId,
            user.AvatarThumbnailUrl,
            ranking?.BestSingle,
            ranking?.SingleRank,
            ranking?.BestAverage,
            ranking?.AverageRank,
            stats,
            recentForm,
            recentStats);
    }
}
