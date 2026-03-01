using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using arabella.Data;
using arabella.Models;
using arabella.Services;

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
    private readonly IPetPhotoService _photoService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(AppDbContext db, IPetPhotoService photoService, ILogger<IndexModel> logger)
    {
        _db = db;
        _photoService = photoService;
        _logger = logger;
    }

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
        // AsNoTracking ensures we get fresh data from DB (including PhotoUrl after upload)
        var list = await _db.Pets.AsNoTracking()
            .Where(p => p.UnitNumber == UnitNumber)
            .OrderBy(p => p.Id)
            .ToListAsync();
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
        _logger.LogInformation("AddPet handler: ContentType={ContentType}, Files.Count={Count}",
            Request.ContentType ?? "(null)", Request.Form.Files.Count);

        if (HttpContext.Session.GetString("Auth") != "1")
            return new UnauthorizedResult();
        if (IsViewOnly)
            return new ForbidResult();

        UnitNumber = unitNumber ?? "";
        var count = await _db.Pets.CountAsync(p => p.UnitNumber == UnitNumber);
        if (count >= MaxPetsPerUnit)
            return BadRequest("Maximum 5 pets per unit.");

        type = (type ?? "").Trim();
        if (type != "Cat" && type != "Dog")
            return BadRequest("Choose Cat or Dog.");

        if (type == "Dog")
        {
            size = (size ?? "").Trim();
            if (size != "Small" && size != "Medium" && size != "Large")
                return BadRequest("Choose dog size.");
            var collar = (collarId ?? "").Trim();
            collar = new string(collar.Where(char.IsDigit).ToArray());
            if (collar.Length > 10)
                return BadRequest("Collar ID: up to 10 digits only.");
            collarId = string.IsNullOrEmpty(collar) ? null : collar;
        }
        else
        {
            size = null;
            collarId = null;
        }

        color = (color ?? "").Trim();
        if (string.IsNullOrEmpty(color))
            return BadRequest("Choose a color.");

        var pet = new Pet
        {
            UnitNumber = UnitNumber,
            Type = type,
            Size = size,
            Color = color,
            PetId = collarId ?? ""
        };
        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();

        // Get file: try "photo" then first file (AJAX multipart can use different key)
        var photo = Request.Form.Files.GetFile("photo");
        if (photo == null && Request.Form.Files.Count > 0)
            photo = Request.Form.Files[0];

        if (photo != null && photo.Length > 0)
        {
            var contentType = string.IsNullOrWhiteSpace(photo.ContentType) ? "image/jpeg" : photo.ContentType;
            if (!IsAllowedImageType(contentType)) contentType = "image/jpeg";

            await using var stream = photo.OpenReadStream();
            if (stream.CanSeek) stream.Position = 0;
            var result = await _photoService.UploadAsync(UnitNumber, pet.Id, stream, contentType);

            if (!string.IsNullOrEmpty(result.Url))
            {
                var updated = await _db.Pets.FindAsync(pet.Id);
                if (updated != null)
                {
                    updated.PhotoUrl = result.Url;
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                _logger.LogWarning("AddPet: photo upload failed. Error={Error}", result.ErrorMessage);
                var message = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() && !string.IsNullOrEmpty(result.ErrorMessage)
                    ? "Photo upload failed. " + result.ErrorMessage
                    : "Photo upload failed. Check Azure Storage settings.";
                return BadRequest(message);
            }
        }

        var listModel = await GetListViewModelAsync();
        return new PartialViewResult
        {
            ViewName = "Units/Pets/_PetList",
            ViewData = new ViewDataDictionary<PetListViewModel>(ViewData, listModel)
        };
    }

    private static bool IsAllowedImageType(string contentType) =>
        contentType is "image/jpeg" or "image/jpg" or "image/png" or "image/gif" or "image/webp";

    public async Task<IActionResult> OnGetPhotoAsync(string unitNumber, int id)
    {
        if (HttpContext.Session.GetString("Auth") != "1")
            return new UnauthorizedResult();
        var pet = await _db.Pets.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.UnitNumber == (unitNumber ?? ""));
        if (pet == null || string.IsNullOrEmpty(pet.PhotoUrl))
            return NotFound();
        var download = await _photoService.TryDownloadAsync(pet.PhotoUrl, HttpContext.RequestAborted);
        if (download == null)
            return NotFound();
        return File(download.Value.Stream, download.Value.ContentType);
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
            if (!string.IsNullOrEmpty(pet.PhotoUrl))
                await _photoService.DeleteAsync(pet.PhotoUrl);
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
