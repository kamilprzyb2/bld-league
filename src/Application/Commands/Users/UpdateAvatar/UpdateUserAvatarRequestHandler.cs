using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Users.UpdateAvatar;

public class UpdateUserAvatarRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserAvatarRequest, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateUserAvatarRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.UserRepository;

        if (!await repository.ExistsAsync(request.UserId))
        {
            return CommandResult.FailGeneral($"Nie znaleziono użytkownika z id: {request.UserId}.");
        }

        var updated = new User
        {
            Id = request.UserId,
            FullName = request.FullName,
            WcaId = request.WcaId,
            AvatarUrl = request.AvatarUrl,
            AvatarThumbnailUrl = request.AvatarThumbnailUrl,
            IsAdmin = request.IsAdmin,
        };

        repository.Update(updated);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok();
    }
}
