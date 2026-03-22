using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages;

public class IndexModel : PageModel
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    public string? WcaId { get; set; }
    public string? Role { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    public void OnGet()
    {
        IsAuthenticated = User.Identity != null;
        if (User.Identity != null)
        {
            UserName = User.FindFirst(ClaimTypes.Name)?.Value;
            WcaId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Role = User.FindFirst(ClaimTypes.Role)?.Value;
            ThumbnailUrl = User.FindFirst("thumbnail")?.Value;
        }
        
        
    }
}