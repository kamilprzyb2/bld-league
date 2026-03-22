using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Users.Create;

/// <summary>
/// Handles creating a new user, rejecting the request if a user with the same WCA ID already exists.
/// </summary>
public class CreateUserRequestHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateUserRequest, CommandResult>
{
    public async Task<CommandResult> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.UserRepository;

        if (await repository.ExistsAsync(u => u.WcaId == request.WcaId))
        {
            return CommandResult.Fail(
                nameof(request.WcaId),
                "Użytkownik z takim WCA ID już istnieje.");
        }

        var user = User.Create(request.FullName, request.WcaId, request.AvatarImageUrl, request.AvatarThumbnailUrl, request.IsAdmin);
        await repository.AddAsync(user);
        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Użytkownik został dodany.");
    }
}
