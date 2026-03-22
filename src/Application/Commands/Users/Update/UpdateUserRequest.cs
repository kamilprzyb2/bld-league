using System.ComponentModel.DataAnnotations;
using BldLeague.Application.Common;
using BldLeague.Application.Validation;
using MediatR;

namespace BldLeague.Application.Commands.Users.Update;

/// <summary>
/// Request to update the profile data of an existing user by their ID, including name, WCA ID, avatar URLs, and admin flag.
/// </summary>
public class UpdateUserRequest : IRequest<CommandResult>
{
    [RequiredPl]
    public string FullName { get; set; } = string.Empty;
    [RequiredPl]
    [RegularExpression(@"\d{4}[A-Z]{4}\d{2}", ErrorMessage = "WCA ID ma niepoprawny format.")]
    public string WcaId { get; set; } = string.Empty;
    public string? AvatarImageUrl { get; set; } = null;
    public string? AvatarThumbnailUrl { get; set; } = null;
    public bool IsAdmin { get; init; } = false;
    public Guid UserId { get; set; }
}
