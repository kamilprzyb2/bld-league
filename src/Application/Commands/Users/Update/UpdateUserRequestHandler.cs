using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Users.Update;

/// <summary>
/// Handles updating a user's profile, rejecting the request if the user is not found or the new WCA ID conflicts with another user.
/// </summary>
public class UpdateUserRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserRequest, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.UserRepository;

        if (!await repository.ExistsAsync(u => u.Id == request.UserId))
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono użytkownika z id: {request.UserId}.");
        }

        if (await repository.ExistsAsync(u => u.WcaId == request.WcaId && u.Id != request.UserId))
        {
            return CommandResult.Fail(
                nameof(request.WcaId),
                "Inny użytkownik z takim WCA ID już istnieje.");
        }

        var user = new User
        {
            Id = request.UserId,
            FullName = request.FullName,
            WcaId = request.WcaId,
            AvatarUrl = request.AvatarImageUrl,
            AvatarThumbnailUrl = request.AvatarThumbnailUrl,
            IsAdmin = request.IsAdmin,
        };

        repository.Update(user);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Zapisano dane użytkownika");
    }
}
