using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Users.Import;

/// <summary>
/// Request to bulk-import users from a list of CSV rows, creating new users or updating existing ones by WCA ID.
/// </summary>
public class ImportUsersRequest : IRequest<ImportResult>
{
    public required List<ImportUserRow> Rows { get; init; }
}

/// <summary>
/// Represents a single CSV row for a user import, carrying the user's full name, WCA ID, admin flag, and optional avatar URLs.
/// </summary>
public class ImportUserRow
{
    public int RowNumber { get; init; }
    public required string FullName { get; init; }
    public required string WcaId { get; init; }
    public bool IsAdmin { get; init; }
    public string? AvatarUrl { get; init; }
    public string? AvatarThumbnailUrl { get; init; }
}
