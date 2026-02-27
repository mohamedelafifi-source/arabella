using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace arabella.Pages.Units.Infractions;

public class IndexModel : PageModel
{
    public string UnitNumber { get; set; } = "";
    public int UnitIndex { get; set; }

    public IActionResult OnGet(string unitNumber, int unitIndex = 0)
    {
        if (HttpContext.Session.GetString("Auth") != "1") return RedirectToPage("/Login");
        UnitNumber = unitNumber ?? "";
        UnitIndex = unitIndex;
        return Page();
    }
}
