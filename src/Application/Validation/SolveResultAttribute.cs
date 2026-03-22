using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BldLeague.Application.Validation;

public class SolveResultAttribute : ValidationAttribute
{
    private static readonly Regex TimeRegex = new(
        @"^(\d+(\.\d{1,2})?|(\d{1,2}):\d{2}(\.\d{1,2})?)$",
        RegexOptions.Compiled
    );
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var str = value as string ?? string.Empty;  // Treat null as empty

        str = str.Trim();

        // Empty is valid
        if (string.IsNullOrEmpty(str))
            return ValidationResult.Success;

        // Check DNF / DNS
        if (str.Equals("DNF", StringComparison.OrdinalIgnoreCase) ||
            str.Equals("DNS", StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Success;
        }

        // Check numeric formats
        if (TimeRegex.IsMatch(str))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            "Nieprawidłowy format wyniku."
        );
    }
}