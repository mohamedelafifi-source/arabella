using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace arabella.Pages;

public class LoginModel : PageModel
{
    private const string PasswordAdmin = "123";
    private const string PasswordViewOnly = "789";

    [BindProperty]
    public string Password { get; set; } = "";

    public string? Message { get; set; }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("Auth") == "1")
            return RedirectToPage("/Home");
        return Page();
    }

    public IActionResult OnPost()
    {
        if (Password == PasswordAdmin)
        {
            HttpContext.Session.SetString("Auth", "1");
            HttpContext.Session.SetString("Role", "Admin");
            return RedirectToPage("/Home");
        }
        if (Password == PasswordViewOnly)
        {
            HttpContext.Session.SetString("Auth", "1");
            HttpContext.Session.SetString("Role", "ViewOnly");
            return RedirectToPage("/Home");
        }
        Message = "كلمة المرور غير صحيحة.";
        return Page();
    }
}
