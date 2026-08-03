using BldLeague.Domain.Entities;

namespace BldLeague.Application.Common;

/// <summary>
/// A match seen from one participant's side: self and opponent names, opponent ID and both scores.
/// Opponent fields are null for BYE matches.
/// </summary>
public record MatchPerspective(
    string SelfFullName,
    string? OpponentFullName,
    Guid? OpponentId,
    int SelfScore,
    int OpponentScore)
{
    /// <summary>
    /// Orients a match from the given user's side. The match must have its
    /// <see cref="Match.UserA"/> / <see cref="Match.UserB"/> navigations loaded.
    /// </summary>
    public static MatchPerspective For(Match match, Guid userId)
    {
        bool isUserA = match.UserAId == userId;
        return new MatchPerspective(
            isUserA ? match.UserA.FullName : match.UserB!.FullName,
            isUserA ? match.UserB?.FullName : match.UserA.FullName,
            isUserA ? match.UserBId : match.UserAId,
            isUserA ? match.UserAScore : match.UserBScore,
            isUserA ? match.UserBScore : match.UserAScore);
    }
}
