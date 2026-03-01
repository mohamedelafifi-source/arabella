namespace arabella.Pages.Units.Pets;

public class PetListViewModel
{
    public List<arabella.Models.Pet> Pets { get; set; } = new();
    public string UnitNumber { get; set; } = "";
    public int UnitIndex { get; set; }
    public bool IsViewOnly { get; set; }
}
