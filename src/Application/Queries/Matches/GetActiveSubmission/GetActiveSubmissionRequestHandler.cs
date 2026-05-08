using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Rounds.GetScrambles;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetActiveSubmission;

public class GetActiveSubmissionRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetActiveSubmissionRequest, ActiveSubmissionDto?>
{
    public async Task<ActiveSubmissionDto?> Handle(GetActiveSubmissionRequest request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var match = await unitOfWork.MatchRepository.GetActiveMatchForUserAsync(request.UserId, utcNow);
        if (match == null)
            return null;

        var opponentName = match.UserAId == request.UserId
            ? match.UserB?.FullName ?? string.Empty
            : match.UserA.FullName;

        var scrambles = match.Round.Scrambles
            .OrderBy(s => s.ScrambleNumber)
            .Select(s => new ScrambleDto { ScrambleNumber = s.ScrambleNumber, Notation = s.Notation })
            .ToList();

        var roundName = $"{match.Round.RoundName} — Sezon {match.Round.Season.SeasonNumber}";
        var hasSubmitted = match.HasUserSubmitted(request.UserId);

        var submittedSolves = hasSubmitted
            ? match.Solves
                .Where(s => s.UserId == request.UserId)
                .OrderBy(s => s.Index)
                .Select(s => s.Result.ToString())
                .ToList()
            : [];

        var submittedAt = match.UserAId == request.UserId
            ? match.UserASubmittedAt
            : match.UserBSubmittedAt;

        return new ActiveSubmissionDto
        {
            MatchId = match.Id,
            RoundId = match.RoundId,
            RoundName = roundName,
            LeagueIdentifier = match.LeagueSeason.League.LeagueIdentifier,
            OpponentName = opponentName,
            HasSubmitted = hasSubmitted,
            SubmittedAt = submittedAt,
            Scrambles = scrambles,
            SubmittedSolves = submittedSolves,
        };
    }
}
