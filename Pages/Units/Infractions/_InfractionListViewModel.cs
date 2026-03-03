namespace arabella.Pages.Units.Infractions;

public class InfractionListViewModel
{
    public List<arabella.Models.Infraction> Infractions { get; set; } = new();
    public string UnitNumber { get; set; } = "";
    public int UnitIndex { get; set; }
    public bool IsViewOnly { get; set; }
}
