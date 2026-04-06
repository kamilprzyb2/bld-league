using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Application.Queries.Matches.GetMatchSummaries;
using BldLeague.Domain.Entities;
using System.Linq;

namespace BldLeague.Web.ViewModels;

public class MatchDetailsViewModel : MatchSummaryViewModel
{
    public Guid SeasonId { get; set; }
    public Guid LeagueId { get; set; }
    public int RoundNumber { get; set; }
    public required string MatchName { get; set; }
    public required string SeasonName { get; set; }
    public required string LeagueName { get; set; }
    public required string RoundName { get; set; }

    public required List<string> SolvesUserA { get; set; }
    public required List<string> SolvesUserB { get; set; }

    public required string UserABest { get; set; }
    public required string UserBBest { get; set; }
    public required string UserAAverage { get; set; }
    public required string UserBAverage { get; set; }

    /// <summary>
    /// Scramble notations indexed 0 to <see cref="Match.SOLVES_PER_MATCH"/>-1.
    /// Null at a given index means the scramble was not found for that position.
    /// </summary>
    public required List<string?> Scrambles { get; set; }

    public static MatchDetailsViewModel FromDto(MatchDetailsDto dto)
    {
        var now = DateTime.Now;
        var status = dto.RoundEndDate < now
            ? MatchStatus.Finished
            : dto.RoundStartDate <= now
                ? MatchStatus.InProgress
                : MatchStatus.Upcoming;

        return new MatchDetailsViewModel
        {
            MatchName = $"{dto.UserAFullName} - {dto.UserBFullName ?? string.Empty}",
            MatchId = dto.Id,
            SeasonId = dto.SeasonId,
            LeagueId = dto.LeagueId,
            RoundNumber = dto.RoundNumber,
            UserAFullName = dto.UserAFullName,
            UserBFullName = dto.UserBFullName,
            UserAScore = dto.UserAScore,
            UserBScore = dto.UserBScore,
            Status = status,
            SeasonName = dto.SeasonName,
            LeagueName = dto.LeagueName,
            RoundName = dto.RoundName,
            SolvesUserA = dto.SolvesUserA.Select(s => s.ToString()).ToList(),
            SolvesUserB = dto.SolvesUserB.Select(s => s.ToString()).ToList(),
            UserABest = dto.UserABest.ToSummaryString(),
            UserBBest = dto.UserBBest.ToSummaryString(),
            UserAAverage = dto.UserAAverage.ToSummaryString(),
            UserBAverage = dto.UserBAverage.ToSummaryString(),
            Scrambles = Enumerable.Range(1, Match.SOLVES_PER_MATCH)
                .Select(i => dto.Scrambles.FirstOrDefault(s => s.ScrambleNumber == i)?.Notation)
                .ToList(),
        };
    }
}
