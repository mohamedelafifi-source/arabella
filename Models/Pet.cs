namespace arabella.Models;

public class Pet
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = "";  // FK
    public string Type { get; set; } = "";        // Cat | Dog
    public string? Size { get; set; }             // Small | Medium | Large (for Dog)
    public string Color { get; set; } = "";       // from hardcoded list
    public string PetId { get; set; } = "";       // 10 - e.g. tag/license, optional
    public byte[]? Photo { get; set; }            // later (Azure Blob)

    public Unit? Unit { get; set; }
}
