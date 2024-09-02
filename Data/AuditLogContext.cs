using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using BookStoreApi.Models;
using BookStoreApi.Attributes;

namespace BookStoreApi.Data;

public class AuditableDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditableDbContext> _logger;

    public AuditableDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor, ILogger<AuditableDbContext> logger)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TrackChanges();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void TrackChanges()
    {
        IEnumerable<EntityEntry> entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Where(e => e.Entity is IAuditableEntity && !(e.Entity is INotAuditableEntity)) // Only audit entities that implement IAuditableEntity and not INotAuditableEntity
            .ToList();

        foreach (EntityEntry entry in entries)
        {
            if (entry.Entity is IAuditableEntity entity)
            {
                var auditLog = new AuditLog
                {
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = entry.Property("ID").CurrentValue is Guid guid ? guid : Guid.Empty,
                    Action = entry.State.ToString(),
                    ChangedBy = GetCurrentUser(),
                    ChangeDate = DateTime.UtcNow,
                    Details = GetEntityDetails(entry) ?? string.Empty
                };

                // Log for debugging
                _logger.LogInformation($"Adding audit log: {auditLog.EntityName}, {auditLog.EntityId}, {auditLog.Action}, {auditLog.Details}");
                Set<AuditLog>().Add(auditLog);
            }
        }
    }

    private string GetEntityDetails(EntityEntry entry)
    {
        var changes = new List<Dictionary<string, object>>();

        var sensitiveProperties = new HashSet<string>(
            entry.Entity.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(ISensitiveAttribute), false).Any())
                .Select(p => p.Name)
        );

        foreach (var property in entry.OriginalValues.Properties)
        {
            string propertyName = property.Name;

            object? originalValue = entry.OriginalValues[property];
            object? currentValue = entry.CurrentValues[property];

            // Only mask sensitive data in the audit log
            object? maskedOriginalValue = sensitiveProperties.Contains(propertyName) ? "****" : originalValue;
            object? maskedCurrentValue = sensitiveProperties.Contains(propertyName) ? "****" : currentValue;

            if (!Equals(originalValue, currentValue))
            {
                var change = new Dictionary<string, object>
                    {
                        {
                            propertyName, new
                            {
                                old_value = maskedOriginalValue,
                                new_value = maskedCurrentValue
                            }
                        }
                    };
                changes.Add(change);
            }
        }

        // Serialize the list of changes to JSON
        var options = new JsonSerializerOptions
        {
            WriteIndented = true // Optional: For pretty printing
        };
        return JsonSerializer.Serialize(changes, options);
    }

    private string GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
    }
}
