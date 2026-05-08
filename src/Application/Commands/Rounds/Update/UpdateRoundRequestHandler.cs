using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.Update;

/// <summary>
/// Handles updating a round's number and dates, validating existence, date order, and round number uniqueness within the season.
/// </summary>
public class UpdateRoundRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRoundRequest, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateRoundRequest request, CancellationToken cancellationToken)
    {
        var round = await unitOfWork.RoundRepository.GetByIdAsync(request.Id);

        if (round == null)
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono rundy z ID: {request.Id}");
        }

        if (request.StartDate > request.EndDate)
        {
            return CommandResult.Fail(
                nameof(request.EndDate),
                "Data końcowa nie może być wcześniejsza niż początkowa");
        }

        if (!await unitOfWork.SeasonRepository.ExistsAsync(round.SeasonId))
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono sezonu z ID: {round.SeasonId}");
        }

        if (await unitOfWork.RoundRepository.ExistsAsync(
                r => r.SeasonId == round.SeasonId
                     && r.RoundNumber == request.RoundNumber
                     && r.Id != request.Id))
        {
            return CommandResult.Fail(
                nameof(request.RoundNumber),
                "Kolejka o podanym numerze już istnieje");
        }

        round.RoundNumber = request.RoundNumber;
        round.StartDate = request.StartDate;
        round.EndDate = request.EndDate;

        unitOfWork.RoundRepository.Update(round);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Kolejka została zaktualizowana");
    }
}
