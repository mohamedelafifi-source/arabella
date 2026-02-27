namespace arabella.Models;

public class Phone
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = "";  // FK
    public string Telephone { get; set; } = "";   // 15

    public Unit? Unit { get; set; }
}
