using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.Create;

/// <summary>
/// Handles creating a new round in a season, validating date order and uniqueness of the round number.
/// </summary>
public class CreateRoundRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateRoundRequest, CommandResult>
{
    public async Task<CommandResult> Handle(CreateRoundRequest request, CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return CommandResult.Fail(
                nameof(request.EndDate),
                "Data końcowa nie może być wcześniejsza niż początkowa");
        }

        if (!await unitOfWork.SeasonRepository.ExistsAsync(request.SeasonId))
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono sezonu z ID: {request.SeasonId}");
        }

        if (await unitOfWork.RoundRepository.ExistsAsync(
                r => r.SeasonId == request.SeasonId && r.RoundNumber == request.RoundNumber))
        {
            return CommandResult.Fail(
                nameof(request.RoundNumber),
                "Kolejka o podanym numerze już istnieje");
        }

        var round = Round.Create(request.SeasonId, request.RoundNumber, request.StartDate, request.EndDate);
        await unitOfWork.RoundRepository.AddAsync(round);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Kolejka została utworzona");
    }
}
