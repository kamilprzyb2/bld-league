namespace BldLeague.Application.Common;

public class ImportResult
{
    public required IReadOnlyList<ImportRowResult> RowResults { get; init; }
    public bool HasErrors => RowResults.Any(r => !r.Success);
    public int SuccessCount => RowResults.Count(r => r.Success);
    public int ErrorCount => RowResults.Count(r => !r.Success);
}
