namespace BldLeague.Application.Common;

public class ImportRowResult
{
    public int RowNumber { get; init; }
    public bool Success { get; init; }
    public required string Message { get; init; }

    public static ImportRowResult Ok(int rowNumber, string message)
        => new() { RowNumber = rowNumber, Success = true, Message = message };

    public static ImportRowResult Fail(int rowNumber, string message)
        => new() { RowNumber = rowNumber, Success = false, Message = message };
}
