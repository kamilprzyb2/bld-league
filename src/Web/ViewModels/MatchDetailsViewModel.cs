using BldLeague.Application.Common;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Domain.Entities;
using BldLeague.Domain.Enums;

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

    public RecordLevel UserABestRecord { get; set; }
    public RecordLevel UserAAverageRecord { get; set; }
    public RecordLevel UserBBestRecord { get; set; }
    public RecordLevel UserBAverageRecord { get; set; }

    /// <summary>
    /// Scramble notations indexed 0 to <see cref="Match.SOLVES_PER_MATCH"/>-1.
    /// Null at a given index means the scramble was not found for that position.
    /// </summary>
    public required List<string?> Scrambles { get; set; }

    public required bool IsRoundFinished { get; set; }

    public static MatchDetailsViewModel FromDto(MatchDetailsDto dto, RoundClock clock)
    {
        return new MatchDetailsViewModel
        {
            IsRoundFinished = clock.IsRoundFinished(dto.RoundEndDate),
            MatchName = $"{dto.UserAFullName} - {dto.UserBFullName ?? string.Empty}",
            MatchId = dto.Id,
            UserAId = dto.UserAId,
            UserBId = dto.UserBId,
            SeasonId = dto.SeasonId,
            LeagueId = dto.LeagueId,
            RoundNumber = dto.RoundNumber,
            UserAFullName = dto.UserAFullName,
            UserBFullName = dto.UserBFullName,
            UserAScore = dto.UserAScore,
            UserBScore = dto.UserBScore,
            Status = ComputeStatus(clock, dto.RoundStartDate, dto.RoundEndDate, dto.BothSidesSubmitted),
            SeasonName = dto.SeasonName,
            LeagueName = dto.LeagueName,
            RoundName = dto.RoundName,
            SolvesUserA = dto.SolvesUserA.Select(s => s.ToString()).ToList(),
            SolvesUserB = dto.SolvesUserB.Select(s => s.ToString()).ToList(),
            UserABest = dto.UserABest.ToSummaryString(),
            UserBBest = dto.UserBBest.ToSummaryString(),
            UserAAverage = dto.UserAAverage.ToSummaryString(),
            UserBAverage = dto.UserBAverage.ToSummaryString(),
            UserABestRecord = dto.UserABestRecord,
            UserAAverageRecord = dto.UserAAverageRecord,
            UserBBestRecord = dto.UserBBestRecord,
            UserBAverageRecord = dto.UserBAverageRecord,
            Scrambles = Enumerable.Range(1, Match.SOLVES_PER_MATCH)
                .Select(i => dto.Scrambles.FirstOrDefault(s => s.ScrambleNumber == i)?.Notation)
                .ToList(),
        };
    }
}
