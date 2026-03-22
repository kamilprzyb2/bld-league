using BldLeague.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BldLeague.Infrastructure.Converters;

public class SolveResultConverter : ValueConverter<SolveResult, int>
{
    public SolveResultConverter() 
        : base(
            v => v.Centiseconds,              // to database
            v => SolveResult.FromCentiseconds(v)) // from database
    { }
}