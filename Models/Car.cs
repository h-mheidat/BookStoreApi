namespace BookStoreApi.Models;

public class Car: INotAuditableEntity
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string VIN { get; set; } = string.Empty;
}
