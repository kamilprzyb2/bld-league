using System.ComponentModel.DataAnnotations;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Application.Validation;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Create;

/// <summary>
/// Request to create a new match between two players in a given league season and round, including all solve results.
/// </summary>
public class CreateMatchRequest : IRequest<CommandResult>, IValidatableObject
{
    [Required][NotEmptyGuid]
    public Guid LeagueSeasonId { get; set; }
    [Required][NotEmptyGuid]
    public Guid RoundId { get; set; }
    [Required][NotEmptyGuid]
    public Guid UserAId { get; set; }

    public Guid UserBId { get; set; }
    [Required]
    public int UserAScore { get; set; }

    public int UserBScore { get; set; }

    public List<SolveDto> UserASolves { get; set; } = new (Match.SOLVES_PER_MATCH);
    public List<SolveDto> UserBSolves { get; set; } = new (Match.SOLVES_PER_MATCH);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserBId != Guid.Empty && UserAId == UserBId)
        {
            yield return new ValidationResult(
                "Zawodnik nie może rozgrywać meczu sam ze sobą.",
                [nameof(UserBId)]);
        }

        if (UserAScore < 0)
        {
            yield return new ValidationResult(
                "Wynik nie może być ujemny.",
                [nameof(UserAScore)]);
        }

        if (UserBScore < 0)
        {
            yield return new ValidationResult(
                "Wynik nie może być ujemny.",
                [nameof(UserBScore)]);
        }
    }
}
