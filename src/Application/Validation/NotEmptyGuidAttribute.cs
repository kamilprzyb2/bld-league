using System.ComponentModel.DataAnnotations;

namespace BldLeague.Application.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class NotEmptyGuidAttribute() : ValidationAttribute("Pole wymagane.")
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is Guid guid && guid != Guid.Empty)
        {
            return ValidationResult.Success;
        }

        var message = FormatErrorMessage(validationContext.DisplayName);
        return new ValidationResult(message);
    }
}