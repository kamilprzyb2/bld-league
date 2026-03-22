using BldLeague.Domain.Interfaces;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public sealed class User : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }
    
    /// <summary>
    /// Full name (e.g. John Doe).
    /// </summary>
    public string FullName { get; init; } = string.Empty;
    
    /// <summary>
    /// World Cube Association ID.
    /// </summary>
    public string WcaId { get; init; } = string.Empty;

    /// <summary>
    /// Optional url to the avatar image.
    /// Null when the user does not have an avatar set.
    /// </summary>
    public string? AvatarUrl { get; init; } = null;
    
    /// <summary>
    /// Optional url to the avatar thumbnail image.
    /// Null when the user does not have an avatar set.
    /// </summary>
    public string? AvatarThumbnailUrl { get; init; } = null;
    
    /// <summary>
    /// Determines whether user has admin access.
    /// </summary>
    public bool IsAdmin { get; init; } = false;
    
    /// <summary>
    /// League season users associated with this user.
    /// </summary>
    public ICollection<LeagueSeasonUser> LeagueSeasonUsers { get; set; } = new List<LeagueSeasonUser>();
    
    /// <summary>
    /// All solves performed by the user.
    /// </summary>
    public ICollection<Solve> Solves { get; set; } = new List<Solve>();
    
    /// <summary>
    /// Matches where this user is UserA.
    /// </summary>
    public ICollection<Match> MatchesAsUserA { get; set; } = new List<Match>();

    /// <summary>
    /// Matches where this user is UserB.
    /// </summary>
    public ICollection<Match> MatchesAsUserB { get; set; } = new List<Match>();
    
    /// <summary>
    /// Round standings of the user.
    /// </summary>
    public ICollection<RoundStanding> RoundStandings { get; set; } = new List<RoundStanding>();
    
    /// <summary>
    /// League season standings of the user.
    /// </summary>
    public ICollection<LeagueSeasonStanding> LeagueSeasonStandings { get; set; } = new List<LeagueSeasonStanding>();
    
    /// <summary>
    /// User factory method.
    /// </summary>
    /// <param name="fullName">Full name (e.g. John Doe).</param>
    /// <param name="wcaId">World Cube Association ID.</param>
    /// <param name="avatarImageUrl">Optional url to the avatar image. Null when the user does not have an avatar set.</param>
    /// <param name="avatarThumbnailUrl">Optional url to the avatar thumbnail image. Null when the user does not have an avatar set.</param>
    /// <param name="isAdmin">Determines whether user has admin access.</param>
    /// <returns>New user.</returns>
    public static User Create(string fullName, string wcaId, string? avatarImageUrl = null, string? avatarThumbnailUrl = null, bool isAdmin = false)
        => new User
        {
            Id = Guid.CreateVersion7(),
            FullName = fullName,
            WcaId = wcaId,
            AvatarUrl = avatarImageUrl,
            AvatarThumbnailUrl = avatarThumbnailUrl,
            IsAdmin = isAdmin
        };
}