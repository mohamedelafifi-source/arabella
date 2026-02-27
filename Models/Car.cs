namespace arabella.Models;

public class Car
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = "";  // FK
    public string CarModel { get; set; } = "";   // 20
    public string CarNumber { get; set; } = "";  // 8

    public Unit? Unit { get; set; }
}
