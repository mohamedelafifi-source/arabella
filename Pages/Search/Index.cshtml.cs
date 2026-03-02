using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using arabella.Data;
using arabella.Models;

namespace arabella.Pages.Search;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    /// <summary>Search criteria: Type (Cat/Dog) is required; Size and Color are optional.</summary>
    public string? PetType { get; set; }
    public string? PetSize { get; set; }
    public string? PetColor { get; set; }

    /// <summary>Units that have at least one pet matching the search.</summary>
    public List<Unit> SearchResults { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return RedirectToPage("/Login");
        return Page();
    }

    public async Task<IActionResult> OnPostSearchByPetAsync(string type, string? size, string? color)
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return RedirectToPage("/Login");

        type = (type ?? "").Trim();
        if (type != "Cat" && type != "Dog")
        {
            TempData["SearchError"] = "اختر نوع الحيوان: قط أو كلب.";
            return RedirectToPage();
        }

        if (type == "Dog")
        {
            size = (size ?? "").Trim();
            if (!string.IsNullOrEmpty(size) && size != "Small" && size != "Medium" && size != "Large")
                size = null;
        }
        else
            size = null;

        color = (color ?? "").Trim();
        var colors = arabella.Pages.Units.Pets.IndexModel.PetColors;
        if (!string.IsNullOrEmpty(color) && !colors.Contains(color, StringComparer.Ordinal))
            color = null;

        var query = _db.Pets.AsNoTracking().Where(p => p.Type == type);
        if (!string.IsNullOrEmpty(size))
            query = query.Where(p => p.Size == size);
        if (!string.IsNullOrEmpty(color))
            query = query.Where(p => p.Color == color);

        var unitNumbers = await query.Select(p => p.UnitNumber).Distinct().ToListAsync();
        var units = await _db.Units.AsNoTracking()
            .Where(u => unitNumbers.Contains(u.UnitNumber))
            .OrderBy(u => u.UnitNumber)
            .ToListAsync();

        PetType = type;
        PetSize = size;
        PetColor = string.IsNullOrEmpty(color) ? null : color;
        SearchResults = units;
        return Page();
    }
}
