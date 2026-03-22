using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Domain.Entities;
using BldLeague.Domain.Scoring;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Common;

public static class MatchSolvesProcessor
{
    /// <summary>
    /// Creates and persists Solve entities for both players, then computes and sets
    /// the match's scores, bests, and averages based on the provided solve inputs.
    /// </summary>
    public static async Task ProcessAsync(
        IUnitOfWork unitOfWork,
        Match match,
        List<SolveDto> userASolves,
        List<SolveDto> userBSolves)
    {
        int userAScore = 0, userBScore = 0;
        int userABest = int.MaxValue, userBBest = int.MaxValue;

        List<Solve> solvesA = [];
        // For walkovers (UserBId is null), no Solve entity is created for UserB — results are
        // tracked as SolveResult values directly to avoid creating a Solve with a dummy UserId.
        List<SolveResult> solvesB = [];

        for (var i = 0; i < Match.SOLVES_PER_MATCH; i++)
        {
            var resultA = SolveResult.FromString(userASolves[i].Result);
            var resultB = SolveResult.FromString(userBSolves.Count > i ? userBSolves[i].Result : null);

            var solveUserA = Solve.Create(resultA, i + 1, match.UserAId, match.Id);
            solvesA.Add(solveUserA);
            await unitOfWork.SolveRepository.AddAsync(solveUserA);

            if (match.UserBId.HasValue)
            {
                var solveUserB = Solve.Create(resultB, i + 1, match.UserBId.Value, match.Id);
                await unitOfWork.SolveRepository.AddAsync(solveUserB);
            }

            solvesB.Add(resultB);

            if (resultA.IsValid || resultB.IsValid)
            {
                if (resultA.IsValid && !resultB.IsValid)
                    userAScore++;
                else if (!resultA.IsValid && resultB.IsValid)
                    userBScore++;
                else if (resultA.Centiseconds < resultB.Centiseconds)
                    userAScore++;
                else if (resultB.Centiseconds < resultA.Centiseconds)
                    userBScore++;
                else
                {
                    userAScore++;
                    userBScore++;
                }
            }

            if (resultA.IsValid) userABest = Math.Min(userABest, resultA.Centiseconds);
            if (resultB.IsValid) userBBest = Math.Min(userBBest, resultB.Centiseconds);
        }

        // Best solve bonus — only awarded when at least one player has a valid solve
        if (userABest < userBBest)
            userAScore++;
        else if (userBBest < userABest)
            userBScore++;
        else if (userABest != int.MaxValue) // both equal and valid → tie
        {
            userAScore++;
            userBScore++;
        }

        match.UserABest = userABest < int.MaxValue
            ? SolveResult.FromCentiseconds(userABest)
            : solvesA.All(s => s.Result.IsDns) ? SolveResult.Dns() : SolveResult.Dnf();
        match.UserBBest = userBBest < int.MaxValue
            ? SolveResult.FromCentiseconds(userBBest)
            : solvesB.All(s => s.IsDns) ? SolveResult.Dns() : SolveResult.Dnf();
        match.UserAAverage = AverageCalculator.CalculateAo5(solvesA.Select(s => s.Result).ToList());
        match.UserBAverage = AverageCalculator.CalculateAo5(solvesB);
        match.UserAScore = userAScore;
        match.UserBScore = userBScore;
    }
}
