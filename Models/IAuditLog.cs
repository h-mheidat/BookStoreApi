using System;

namespace BookStoreApi.Models;

public interface IAuditLog
{
  string EntityName { get; set; }
    Guid EntityId { get; set; }
    string Action { get; set; }
    string ChangedBy { get; set; }
    DateTime ChangeDate { get; set; }
    public string Details { get; set; }
}
