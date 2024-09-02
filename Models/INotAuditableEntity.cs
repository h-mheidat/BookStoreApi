namespace BookStoreApi.Models;

public interface INotAuditableEntity
{
     Guid ID { get; set; }
}
