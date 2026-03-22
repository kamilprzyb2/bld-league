using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.UpdateScrambles;

/// <summary>
/// Handles replacing a round's scrambles by deleting existing ones and inserting any non-empty notations provided.
/// </summary>
public class UpdateRoundScramblesRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRoundScramblesRequest, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateRoundScramblesRequest request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.RoundRepository.ExistsAsync(request.RoundId))
            return CommandResult.FailGeneral($"Nie znaleziono kolejki z ID: {request.RoundId}");

        await unitOfWork.ScrambleRepository.DeleteByRoundIdAsync(request.RoundId);

        for (var i = 0; i < request.Notations.Length; i++)
        {
            var notation = request.Notations[i];
            if (!string.IsNullOrWhiteSpace(notation))
                await unitOfWork.ScrambleRepository.AddAsync(Scramble.Create(request.RoundId, i + 1, notation));
        }

        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Scramble zostały zaktualizowane");
    }
}
