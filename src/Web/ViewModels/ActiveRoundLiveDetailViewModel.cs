using BldLeague.Application.Queries.Rounds.GetActiveRoundLiveDetail;
using BldLeague.Web.Helpers;

namespace BldLeague.Web.ViewModels;

/// <summary>
/// View model rendering the three-section live view of an active (or not-yet-started) round.
/// </summary>
public class ActiveRoundLiveDetailViewModel
{
    public required string StartDate { get; set; }
    public required string EndDate { get; set; }
    public required List<ActiveRoundLiveRowViewModel> FinishedRows { get; set; }
    public required List<ActiveRoundLiveRowViewModel> SubmittedAloneRows { get; set; }
    public required List<ActiveRoundLiveRowViewModel> PendingRows { get; set; }

    public static ActiveRoundLiveDetailViewModel FromDto(ActiveRoundLiveDetailDto dto)
    {
        return new ActiveRoundLiveDetailViewModel
        {
            StartDate = dto.StartDate.ToDisplayDate(),
            EndDate = dto.EndDate.ToDisplayDate(),
            FinishedRows = dto.FinishedRows
                .Select(ActiveRoundLiveRowViewModel.FromFinishedDto)
                .ToList(),
            SubmittedAloneRows = dto.SubmittedAloneRows
                .Select(ActiveRoundLiveRowViewModel.FromIdentityDto)
                .ToList(),
            PendingRows = dto.PendingRows
                .Select(ActiveRoundLiveRowViewModel.FromIdentityDto)
                .ToList(),
        };
    }
}
