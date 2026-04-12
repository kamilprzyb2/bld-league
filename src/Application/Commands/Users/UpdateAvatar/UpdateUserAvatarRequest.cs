using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Users.UpdateAvatar;

public class UpdateUserAvatarRequest : IRequest<CommandResult>
{
    public Guid UserId { get; init; }
    public string? AvatarUrl { get; init; }
    public string? AvatarThumbnailUrl { get; init; }
}
