namespace arabella.Models;

public class Pet
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = "";  // FK
    public string Type { get; set; } = "";        // Cat | Dog
    public string? Size { get; set; }             // Small | Medium | Large (for Dog)
    public string Color { get; set; } = "";       // from hardcoded list
    public string PetId { get; set; } = "";       // collar/tag ID, optional
    public string? PhotoUrl { get; set; }        // Azure Blob URL

    public Unit? Unit { get; set; }
}
