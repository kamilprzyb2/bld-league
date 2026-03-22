using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.UpdateScrambles;

/// <summary>
/// Request to replace all scramble notations for a specific round.
/// </summary>
public class UpdateRoundScramblesRequest : IRequest<CommandResult>
{
    public required Guid RoundId { get; init; }

    /// <summary>
    /// Scramble notations indexed 0 to <see cref="Domain.Entities.Match.SOLVES_PER_MATCH"/>-1.
    /// Index 0 = scramble number 1. Null or empty entries delete that scramble.
    /// </summary>
    public required string?[] Notations { get; init; }
}
