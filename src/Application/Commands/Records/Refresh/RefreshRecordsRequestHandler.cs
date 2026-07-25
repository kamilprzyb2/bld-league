using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using BldLeague.Domain.Enums;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Commands.Records.Refresh;

/// <summary>
/// Handles recomputing PR (personal record) and LR (site-wide record) levels on all round standings.
/// Sweeps rounds chronologically by (SeasonNumber, RoundNumber); results within one round are simultaneous,
/// ties (&lt;=) count, and only valid results can earn or hold a record.
/// </summary>
public class RefreshRecordsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshRecordsRequest, CommandResult>
{
    public async Task<CommandResult> Handle(RefreshRecordsRequest request, CancellationToken cancellationToken)
    {
        var standings = await unitOfWork.RoundStandingRepository.GetAllWithRoundAsync();

        var personalBestSingles = new Dictionary<Guid, int>();
        var personalBestAverages = new Dictionary<Guid, int>();
        int? globalBestSingle = null;
        int? globalBestAverage = null;

        var changed = new List<RoundStanding>();

        var roundGroups = standings
            .OrderBy(rs => rs.Round.Season.SeasonNumber)
            .ThenBy(rs => rs.Round.RoundNumber)
            .GroupBy(rs => new { rs.Round.Season.SeasonNumber, rs.Round.RoundNumber });

        foreach (var roundGroup in roundGroups)
        {
            var rows = roundGroup.ToList();

            // Fallback records for the no-prior-record case: only the round's best result(s) (+ties) earn LR.
            int? roundBestSingle = rows
                .Where(rs => rs.Best.IsValid)
                .Select(rs => (int?)rs.Best.Centiseconds)
                .Min();
            int? roundBestAverage = rows
                .Where(rs => rs.Average.IsValid)
                .Select(rs => (int?)rs.Average.Centiseconds)
                .Min();

            foreach (var standing in rows)
            {
                int? personalSingle = personalBestSingles.TryGetValue(standing.UserId, out var pbs) ? pbs : null;
                int? personalAverage = personalBestAverages.TryGetValue(standing.UserId, out var pba) ? pba : null;

                var bestRecord = ComputeRecordLevel(standing.Best, personalSingle, globalBestSingle, roundBestSingle);
                var averageRecord = ComputeRecordLevel(standing.Average, personalAverage, globalBestAverage, roundBestAverage);

                if (standing.BestRecord != bestRecord || standing.AverageRecord != averageRecord)
                {
                    standing.BestRecord = bestRecord;
                    standing.AverageRecord = averageRecord;
                    changed.Add(standing);
                }
            }

            // Merge the whole round into the running minima only after every level in the round is computed.
            foreach (var standing in rows)
            {
                if (standing.Best.IsValid)
                {
                    if (!personalBestSingles.TryGetValue(standing.UserId, out var currentSingle)
                        || standing.Best.Centiseconds < currentSingle)
                        personalBestSingles[standing.UserId] = standing.Best.Centiseconds;
                    if (globalBestSingle == null || standing.Best.Centiseconds < globalBestSingle.Value)
                        globalBestSingle = standing.Best.Centiseconds;
                }

                if (standing.Average.IsValid)
                {
                    if (!personalBestAverages.TryGetValue(standing.UserId, out var currentAverage)
                        || standing.Average.Centiseconds < currentAverage)
                        personalBestAverages[standing.UserId] = standing.Average.Centiseconds;
                    if (globalBestAverage == null || standing.Average.Centiseconds < globalBestAverage.Value)
                        globalBestAverage = standing.Average.Centiseconds;
                }
            }
        }

        await unitOfWork.BeginTransactionAsync();
        foreach (var standing in changed)
        {
            unitOfWork.RoundStandingRepository.Update(standing);
        }
        await unitOfWork.SaveAsync();
        await unitOfWork.CommitTransactionAsync();

        return CommandResult.Ok("Zaktualizowano rekordy (PR/LR)");
    }

    private static RecordLevel ComputeRecordLevel(SolveResult result, int? personalBest, int? globalBest, int? roundBest)
    {
        if (!result.IsValid)
            return RecordLevel.None;

        var isLeagueRecord = globalBest != null
            ? result.Centiseconds <= globalBest.Value
            : roundBest != null && result.Centiseconds <= roundBest.Value;
        if (isLeagueRecord)
            return RecordLevel.League;

        if (personalBest == null || result.Centiseconds <= personalBest.Value)
            return RecordLevel.Personal;

        return RecordLevel.None;
    }
}
