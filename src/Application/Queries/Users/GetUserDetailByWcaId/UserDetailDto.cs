namespace BldLeague.Application.Queries.Users.GetUserDetailByWcaId;

/// <summary>
/// Full detail data transfer object for a user, including WCA ID, avatar URLs, and admin flag.
/// </summary>
public class UserDetailDto
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string WcaId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarThumbnailUrl { get; set; }
    public bool IsAdmin { get; set; }
}
