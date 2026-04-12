using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Users.UpdateAvatar;

public class UpdateUserAvatarRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserAvatarRequest, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateUserAvatarRequest request, CancellationToken cancellationToken)
    {
        await unitOfWork.UserRepository.UpdateAvatarAsync(request.UserId, request.AvatarUrl, request.AvatarThumbnailUrl, cancellationToken);
        return CommandResult.Ok();
    }
}
