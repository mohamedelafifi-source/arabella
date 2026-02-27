namespace arabella.Models;

public class Infraction
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = "";  // FK
    public string Description { get; set; } = ""; // 20
    public DateTime Date { get; set; }

    public Unit? Unit { get; set; }
}
