namespace BookStoreApi.Models;

public interface IAuditableEntity
{
    Guid ID { get; set; }
}