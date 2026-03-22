using System.ComponentModel.DataAnnotations;
using BldLeague.Application.Common;
using BldLeague.Application.Validation;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.Update;

/// <summary>
/// Request to update the round number and date range of an existing round by its ID.
/// </summary>
public class UpdateRoundRequest : IRequest<CommandResult>, IValidatableObject
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Numer musi być większy od 0")]
    public int RoundNumber { get; set; }
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
