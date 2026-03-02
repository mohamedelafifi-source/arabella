using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace arabella.Pages;

public class AboutModel : PageModel
{
    public string Version { get; private set; } = "";
    public string EnvironmentName { get; private set; } = "";

    public void OnGet()
    {
        if (HttpContext.Session.GetString("Auth") != "1")
        {
            Response.Redirect("/Login");
            return;
        }

        var asm = Assembly.GetExecutingAssembly();
        Version = asm.GetName().Version?.ToString() ?? "1.0.0";
        EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    }
}