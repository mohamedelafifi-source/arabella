using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace arabella.Pages.Utilities;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return RedirectToPage("/Login");
        return Page();
    }
}
