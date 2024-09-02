namespace BookStoreApi.Models;

public class AuditLog: IAuditLog
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangeDate { get; set; }
    public string Details { get; set; } = string.Empty;
}
