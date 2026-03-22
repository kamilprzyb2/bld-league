using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Users.Delete;

/// <summary>
/// Handles deleting a user, returning a failure result if the user does not exist.
/// </summary>
public class DeleteUserRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteUserRequest, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.UserRepository;
        var user = await repository.GetByIdAsync(request.Id);

        if (user == null)
            return CommandResult.FailGeneral($"Nie znaleziono użytkownika z ID: {request.Id}.");

        repository.Delete(user);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Usunięto użytkownika.");
    }
}
