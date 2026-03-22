using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Leagues.Create;

/// <summary>
/// Handles creating a new league, rejecting the request if a league with the same identifier already exists.
/// </summary>
public class CreateLeagueRequestHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateLeagueRequest, CommandResult>
{
    public async Task<CommandResult> Handle(CreateLeagueRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.LeagueRepository;

        if (await repository.ExistsAsync(l => l.LeagueIdentifier == request.LeagueIdentifier))
        {
            return CommandResult.Fail(
                nameof(request.LeagueIdentifier),
                "Liga z podanym identyfikatorem już istnieje.");
        }

        var league = League.Create(request.LeagueIdentifier);
        await repository.AddAsync(league);
        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Liga została utworzona.");
    }
}
