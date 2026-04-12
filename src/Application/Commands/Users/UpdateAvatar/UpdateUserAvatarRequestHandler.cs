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

        var existing = await repository.GetByIdAsync(request.UserId);
        if (existing == null)
        {
            return CommandResult.FailGeneral($"Nie znaleziono użytkownika z id: {request.UserId}.");
        }

        var updated = new User
        {
            Id = existing.Id,
            FullName = existing.FullName,
            WcaId = existing.WcaId,
            AvatarUrl = request.AvatarUrl,
            AvatarThumbnailUrl = request.AvatarThumbnailUrl,
            IsAdmin = existing.IsAdmin,
        };

        repository.Update(updated);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok();
    }
}
