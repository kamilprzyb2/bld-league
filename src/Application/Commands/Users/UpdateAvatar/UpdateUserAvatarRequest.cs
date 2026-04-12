using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Users.UpdateAvatar;

public class UpdateUserAvatarRequest : IRequest<CommandResult>
{
    public Guid UserId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarThumbnailUrl { get; set; }
}
