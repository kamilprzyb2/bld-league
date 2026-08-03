using BldLeague.Application.Queries.Users.GetAll;

namespace BldLeague.Web.ViewModels;

public record UserDatalistViewModel(string Id, IEnumerable<UserSummaryDto> Users);
