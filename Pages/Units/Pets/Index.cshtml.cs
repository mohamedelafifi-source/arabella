using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using arabella.Data;
using arabella.Models;

namespace arabella.Pages.Units.Pets;

public class IndexModel : PageModel
{
    private const int MaxPetsPerUnit = 5;
    public static readonly string[] PetColors = new[]
    {
        "White", "Beige", "Gray", "Light Gold", "Dark Gold",
        "Brown", "Black", "White and brown", "White and black"
    };

    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public string UnitNumber { get; set; } = "";
    public int UnitIndex { get; set; }
    public List<Pet> Pets { get; set; } = new();
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
        Pets = await _db.Pets.Where(p => p.UnitNumber == UnitNumber).OrderBy(p => p.Id).ToListAsync();
        return Page();
    }

    private async Task<PetListViewModel> GetListViewModelAsync()
    {
        var list = await _db.Pets.Where(p => p.UnitNumber == UnitNumber).OrderBy(p => p.Id).ToListAsync();
        return new PetListViewModel
        {
            Pets = list,
            UnitNumber = UnitNumber,
            UnitIndex = UnitIndex,
            IsViewOnly = IsViewOnly
        };
    }

    public async Task<IActionResult> OnPostAddPetAsync(string unitNumber, string type, string? size, string? color, string? collarId)
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return new UnauthorizedResult();
        if (IsViewOnly)
            return new ForbidResult();

        UnitNumber = unitNumber ?? "";
        var count = await _db.Pets.CountAsync(p => p.UnitNumber == UnitNumber);
        if (count >= MaxPetsPerUnit)
            return BadRequest("الحد الأقصى 5 حيوانات لكل وحدة.");

        type = (type ?? "").Trim();
        if (type != "Cat" && type != "Dog")
            return BadRequest("اختر قط أو كلب.");

        if (type == "Dog")
        {
            size = (size ?? "").Trim();
            if (size != "Small" && size != "Medium" && size != "Large")
                return BadRequest("اختر حجم الكلب.");
            var collar = (collarId ?? "").Trim();
            collar = new string(collar.Where(char.IsDigit).ToArray());
            if (collar.Length > 10)
                return BadRequest("رقم الطوق حتى 10 أرقام فقط.");
            collarId = string.IsNullOrEmpty(collar) ? null : collar;
        }
        else
        {
            size = null;
            collarId = null;
        }

        color = (color ?? "").Trim();
        if (string.IsNullOrEmpty(color))
            return BadRequest("اختر اللون.");

        _db.Pets.Add(new Pet
        {
            UnitNumber = UnitNumber,
            Type = type,
            Size = size,
            Color = color,
            PetId = collarId ?? ""
        });
        await _db.SaveChangesAsync();

        var listModel = await GetListViewModelAsync();
        return new PartialViewResult
        {
            ViewName = "Units/Pets/_PetList",
            ViewData = new ViewDataDictionary<PetListViewModel>(ViewData, listModel)
        };
    }

    public async Task<IActionResult> OnPostDeletePetAsync(string unitNumber, int id)
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return new UnauthorizedResult();
        if (IsViewOnly)
            return new ForbidResult();

        UnitNumber = unitNumber ?? "";
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id && p.UnitNumber == UnitNumber);
        if (pet != null)
        {
            _db.Pets.Remove(pet);
            await _db.SaveChangesAsync();
        }

        var listModel = await GetListViewModelAsync();
        return new PartialViewResult
        {
            ViewName = "Units/Pets/_PetList",
            ViewData = new ViewDataDictionary<PetListViewModel>(ViewData, listModel)
        };
    }
}
