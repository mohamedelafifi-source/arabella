using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using arabella.Data;
using arabella.Models;

namespace arabella.Pages.Units;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public Unit? Current { get; set; }
    public int? CurrentIndex { get; set; }
    public int TotalCount { get; set; }
    public bool IsViewOnly => HttpContext.Session.GetString("Role") == "ViewOnly";

    public async Task<IActionResult> OnGetAsync(int? index, string? unitNumber)
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return RedirectToPage("/Login");

        var list = await _db.Units.OrderBy(u => u.UnitNumber).ToListAsync();
        TotalCount = list.Count;

        if (TotalCount == 0)
        {
            Current = null;
            CurrentIndex = null;
            return Page();
        }

        var i = index ?? 0;
        if (!string.IsNullOrWhiteSpace(unitNumber))
        {
            var idx = list.FindIndex(u => string.Equals(u.UnitNumber, unitNumber.Trim(), StringComparison.Ordinal));
            if (idx >= 0) i = idx;
        }
        if (i < 0) { CurrentIndex = -1; Current = null; return Page(); } // Add new
        if (i >= TotalCount) i = TotalCount - 1;
        CurrentIndex = i;
        Current = list[i];
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(Unit input)
    {
        if (IsViewOnly) return RedirectToPage();
        var unitNumber = input.UnitNumber?.Trim();
        if (string.IsNullOrEmpty(unitNumber)) return RedirectToPage();

        var isNewUnit = string.Equals(Request.Form["IsNewUnit"].FirstOrDefault(), "true", StringComparison.OrdinalIgnoreCase);
        var exists = await _db.Units.FindAsync(unitNumber);

        if (isNewUnit && exists != null)
        {
            TempData["UnitNumberError"] = "رقم الوحدة مستخدم مسبقاً. اختر رقماً آخر.";
            return RedirectToPage(new { index = -1 });
        }

        if (exists != null)
        {
            exists.OwnerName = input.OwnerName ?? "";
            exists.OwnerId = input.OwnerId ?? "";
            exists.OwnerMailAddress = input.OwnerMailAddress ?? "";
            exists.UserName = input.UserName ?? "";
            exists.UserId = input.UserId ?? "";
            exists.UserMailAddress = input.UserMailAddress ?? "";
            exists.SpouseName = input.SpouseName ?? "";
            exists.SpouseId = input.SpouseId ?? "";
            exists.Experience = input.Experience ?? "";
        }
        else
        {
            _db.Units.Add(new Unit
            {
                UnitNumber = unitNumber,
                OwnerName = input.OwnerName ?? "",
                OwnerId = input.OwnerId ?? "",
                OwnerMailAddress = input.OwnerMailAddress ?? "",
                UserName = input.UserName ?? "",
                UserId = input.UserId ?? "",
                UserMailAddress = input.UserMailAddress ?? "",
                SpouseName = input.SpouseName ?? "",
                SpouseId = input.SpouseId ?? "",
                Experience = input.Experience ?? ""
            });
        }
        await _db.SaveChangesAsync();
        var list = await _db.Units.OrderBy(u => u.UnitNumber).ToListAsync();
        var idx = list.FindIndex(u => u.UnitNumber == unitNumber);
        return RedirectToPage(new { index = idx >= 0 ? idx : 0 });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string unitNumber)
    {
        if (IsViewOnly) return RedirectToPage();
        var u = await _db.Units.FindAsync(unitNumber);
        if (u != null)
        {
            _db.Units.Remove(u);
            await _db.SaveChangesAsync();
        }
        var list = await _db.Units.OrderBy(x => x.UnitNumber).ToListAsync();
        var idx = list.Count > 0 ? list.Count - 1 : 0;
        return RedirectToPage(new { index = list.Count == 0 ? -1 : idx });
    }

    public IActionResult OnPostExit()
    {
        return RedirectToPage("/Home");
    }
}
