using System.ComponentModel.DataAnnotations;
using BldLeague.Application.Common;
using BldLeague.Application.Validation;
using MediatR;

namespace BldLeague.Application.Commands.Users.Create;

/// <summary>
/// Request to create a new user with their full name, WCA ID, optional avatar URLs, and admin flag.
/// </summary>
public class CreateUserRequest : IRequest<CommandResult>
{
    [RequiredPl]
    public string FullName { get; set; } = string.Empty;
    [RequiredPl]
    [RegularExpression(@"\d{4}[A-Z]{4}\d{2}", ErrorMessage = "WCA ID ma niepoprawny format.")]
    public string WcaId { get; set; } = string.Empty;
    public string? AvatarImageUrl { get; set; } = null;
    public string? AvatarThumbnailUrl { get; set; } = null;
    public bool IsAdmin { get; init; } = false;
}
