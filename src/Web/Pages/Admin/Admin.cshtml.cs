using BldLeague.Web.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin;

[AdminOnly]
public class Admin : PageModel
{
    public void OnGet()
    {
        
    }
}