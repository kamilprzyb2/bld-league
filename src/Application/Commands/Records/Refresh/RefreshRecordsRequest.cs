using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Records.Refresh;

/// <summary>
/// Request to recompute PR/site-record levels on all round standings (full idempotent sweep).
/// </summary>
public class RefreshRecordsRequest : IRequest<CommandResult>;
