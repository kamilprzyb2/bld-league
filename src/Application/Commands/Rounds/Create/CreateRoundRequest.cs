using System.ComponentModel.DataAnnotations;
using BldLeague.Application.Common;
using BldLeague.Application.Validation;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.Create;

/// <summary>
/// Request to create a new round in a season with a round number and date range.
/// </summary>
public class CreateRoundRequest : IRequest<CommandResult>, IValidatableObject
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Numer musi być większy od 0")]
    public int RoundNumber { get; set; }
    public Guid SeasonId { get; set; }
    [RequiredPl]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    [RequiredPl] [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
        {
            yield return new ValidationResult(
                "Data końcowa musi być późniejsza niż data początkowa",
                [nameof(EndDate)]);
        }
    }
}
