namespace arabella.Models;

public class Child
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = "";  // FK
    public string Name { get; set; } = "";        // 40
    public string Telephone { get; set; } = "";   // 15

    public Unit? Unit { get; set; }
}
