namespace BldLeague.Application.Queries.Users.GetAll;

/// <summary>
/// Summary data transfer object for a user, including WCA ID, admin flag, and avatar URLs.
/// </summary>
public record UserSummaryDto(Guid Id, string FullName, string WcaId, bool IsAdmin, string? AvatarUrl, string? AvatarThumbnailUrl);
