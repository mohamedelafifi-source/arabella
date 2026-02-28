using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace arabella.Pages;

public class HomeModel : PageModel
{
    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return RedirectToPage("/Login");
        return Page();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Login");
    }
}
