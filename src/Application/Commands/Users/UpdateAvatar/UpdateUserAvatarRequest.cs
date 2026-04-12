using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Users.UpdateAvatar;

public class UpdateUserAvatarRequest : IRequest<CommandResult>
{
    public required Guid UserId { get; init; }
    public required string FullName { get; init; }
    public required string WcaId { get; init; }
    public bool IsAdmin { get; init; }
    public string? AvatarUrl { get; init; }
    public string? AvatarThumbnailUrl { get; init; }
}
