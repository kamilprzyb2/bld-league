using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Account;

public class Login : PageModel
{
    public IActionResult OnGet()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/"
        }, "WCA");
    }
}