using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.PlayerRankings.Refresh;

public class RefreshPlayerRankingsRequest : IRequest<CommandResult>;
