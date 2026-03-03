namespace arabella.Models;

public class Infraction
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = "";  // FK
    public DateTime Date { get; set; }
    public string Type { get; set; } = "";  // from fixed list (e.g. حيوانات طليقة، تسرب مياه، إزعاج، مواقف السيارات)

    public Unit? Unit { get; set; }
}
