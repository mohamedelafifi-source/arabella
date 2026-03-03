using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using arabella.Data;
using arabella.Models;

namespace arabella.Pages.Units.Infractions;

public class IndexModel : PageModel
{
    private const int MaxInfractionsPerUnit = 8;

    /// <summary>Fixed infraction types in Arabic (Runaway pets, Water spill, Noise, Car parking).</summary>
    public static readonly string[] InfractionTypes = new[]
    {
        "حيوانات طليقة",
        "تسرب مياه",
        "إزعاج",
        "مواقف السيارات"
    };

    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public string UnitNumber { get; set; } = "";
    public int UnitIndex { get; set; }
    public List<Infraction> Infractions { get; set; } = new();
    public bool IsViewOnly => HttpContext.Session.GetString("Role") == "ViewOnly";

    public async Task<IActionResult> OnGetAsync(string unitNumber, int unitIndex = 0)
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return RedirectToPage("/Login");

        var unit = await _db.Units.FindAsync(unitNumber ?? "");
        if (unit == null)
            return RedirectToPage("/Units/Index");

        UnitNumber = unitNumber ?? "";
        UnitIndex = unitIndex;
        Infractions = await _db.Infractions
            .Where(i => i.UnitNumber == UnitNumber)
            .OrderByDescending(i => i.Date)
            .ThenBy(i => i.Id)
            .ToListAsync();
        return Page();
    }

    private async Task<InfractionListViewModel> GetListViewModelAsync()
    {
        var list = await _db.Infractions.AsNoTracking()
            .Where(i => i.UnitNumber == UnitNumber)
            .OrderByDescending(i => i.Date)
            .ThenBy(i => i.Id)
            .ToListAsync();
        return new InfractionListViewModel
        {
            Infractions = list,
            UnitNumber = UnitNumber,
            UnitIndex = UnitIndex,
            IsViewOnly = IsViewOnly
        };
    }

    public async Task<IActionResult> OnPostAddInfractionAsync(string unitNumber, string? dateStr, string? type)
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return new UnauthorizedResult();
        if (IsViewOnly)
            return new ForbidResult();

        UnitNumber = unitNumber ?? "";
        var count = await _db.Infractions.CountAsync(i => i.UnitNumber == UnitNumber);
        if (count >= MaxInfractionsPerUnit)
            return BadRequest("الحد الأقصى 8 مخالفات لهذه الوحدة.");

        dateStr = (dateStr ?? "").Trim();
        if (string.IsNullOrEmpty(dateStr))
            return BadRequest("اختر التاريخ.");
        // Parse date picker (yyyy-MM-dd) or DD/MM/YY / DD/MM/YYYY
        if (!DateTime.TryParseExact(dateStr, new[] { "yyyy-MM-dd", "dd/MM/yy", "dd/MM/yyyy", "d/M/yy", "d/M/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateParsed))
            return BadRequest("صيغة التاريخ غير صحيحة.");
        type = (type ?? "").Trim();
        if (string.IsNullOrEmpty(type) || !InfractionTypes.Contains(type, StringComparer.Ordinal))
            return BadRequest("اختر نوع المخالفة.");

        var infraction = new Infraction
        {
            UnitNumber = UnitNumber,
            Date = dateParsed.Date,
            Type = type
        };
        _db.Infractions.Add(infraction);
        await _db.SaveChangesAsync();

        var listModel = await GetListViewModelAsync();
        return new PartialViewResult
        {
            ViewName = "Units/Infractions/_InfractionList",
            ViewData = new ViewDataDictionary<InfractionListViewModel>(ViewData, listModel)
        };
    }

    public async Task<IActionResult> OnPostDeleteInfractionAsync(string unitNumber, int id)
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return new UnauthorizedResult();
        if (IsViewOnly)
            return new ForbidResult();

        UnitNumber = unitNumber ?? "";
        var infraction = await _db.Infractions.FirstOrDefaultAsync(i => i.Id == id && i.UnitNumber == UnitNumber);
        if (infraction != null)
        {
            _db.Infractions.Remove(infraction);
            await _db.SaveChangesAsync();
        }

        var listModel = await GetListViewModelAsync();
        return new PartialViewResult
        {
            ViewName = "Units/Infractions/_InfractionList",
            ViewData = new ViewDataDictionary<InfractionListViewModel>(ViewData, listModel)
        };
    }
}
