namespace arabella.Models;

public class Pet
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = "";  // FK
    public string Type { get; set; } = "";        // 30
    public string PetId { get; set; } = "";        // 10 - e.g. tag/license
    public byte[]? Photo { get; set; }             // later

    public Unit? Unit { get; set; }
}
