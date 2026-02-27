namespace arabella.Models;

public class Unit
{
    public string UnitNumber { get; set; } = "";  // PK, max 10
    public string OwnerName { get; set; } = "";   // 40
    public string OwnerId { get; set; } = "";     // 20
    public string OwnerMailAddress { get; set; } = "";  // 30
    public string UserName { get; set; } = "";     // 40
    public string UserId { get; set; } = "";      // 20
    public string UserMailAddress { get; set; } = "";   // 30
    public string SpouseName { get; set; } = "";  // 40
    public string SpouseId { get; set; } = "";    // 20
    public string Experience { get; set; } = "";  // 40 - notes

    public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    public ICollection<Car> Cars { get; set; } = new List<Car>();
    public ICollection<Child> Children { get; set; } = new List<Child>();
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    public ICollection<Infraction> Infractions { get; set; } = new List<Infraction>();
}
