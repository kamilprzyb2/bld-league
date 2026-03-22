using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Seasons.Create;

/// <summary>
/// Handles creating a new season, rejecting the request if a season with the same number already exists.
/// </summary>
public class CreateSeasonRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSeasonRequest, CommandResult>
{
    public async Task<CommandResult> Handle(CreateSeasonRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.SeasonRepository;

        if (await repository.ExistsAsync(s => s.SeasonNumber == request.SeasonNumber))
            return CommandResult.Fail(nameof(request.SeasonNumber), "Sezon z tym numerem już istnieje.");

        var season = Season.Create(request.SeasonNumber);
        await repository.AddAsync(season);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok($"Sezon {request.SeasonNumber} został utworzony.");
    }
}
