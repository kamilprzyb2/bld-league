using System.ComponentModel.DataAnnotations;

namespace BldLeague.Application.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class RequiredPlAttribute : RequiredAttribute
{
    public RequiredPlAttribute()
    {
        ErrorMessage = "Pole jest wymagane.";
    }
}