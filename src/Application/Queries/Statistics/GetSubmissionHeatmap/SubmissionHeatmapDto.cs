namespace BldLeague.Application.Queries.Statistics.GetSubmissionHeatmap;

/// <summary>
/// Submission counts bucketed by day-of-week and hour-of-day in the configured league time zone.
/// Day index 0 = Monday, 6 = Sunday (Polish convention).
/// </summary>
/// <param name="Matrix">[day, hour] count matrix. Rows: 7 (Mon..Sun), Cols: 24.</param>
/// <param name="MaxCount">Maximum cell value across the matrix (for intensity normalisation).</param>
/// <param name="IncludedMatches">Finished matches with at least one non-null submission timestamp.</param>
/// <param name="TotalMatches">Total finished matches in scope.</param>
public record SubmissionHeatmapDto(
    int[,] Matrix,
    int MaxCount,
    int IncludedMatches,
    int TotalMatches);
