using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using System.Text.Json;
using BookStoreApi.Models;
using BookStoreApi.Attributes;

namespace BookStoreApi.Data
{
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
                .Where(e => e.Entity is IAuditableEntity)
                .Where(p => p.Entity.GetType().GetCustomAttributes(typeof(ISensitiveAttribute), false).Any() == false)
                .ToList();

            foreach (EntityEntry entry in entries)
            {
                if (entry.Entity is IAuditableEntity entity)
                {
                    AuditLog auditLog = new AuditLog
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
            List<Dictionary<string, object>> changes = new List<Dictionary<string, object>>();

            foreach (var property in entry.OriginalValues.Properties)
            {
                string propertyName = property.Name;
                PropertyInfo? propertyInfo = entry.Entity.GetType().GetProperty(propertyName);

                object? originalValue = entry.OriginalValues[property];
                object? currentValue = entry.CurrentValues[property];

                // Use the custom value provider to handle sensitive data masking
                object? maskedOriginalValue = SensitiveValueProvider.GetMaskedValue(propertyInfo, originalValue);
                object? maskedCurrentValue = SensitiveValueProvider.GetMaskedValue(propertyInfo, currentValue);

                if (!Equals(originalValue, currentValue))
                {
                    Dictionary<string, object> change = new Dictionary<string, object>
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
            JsonSerializerOptions options = new JsonSerializerOptions
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

    public static class SensitiveValueProvider
    {
        public static object GetMaskedValue(PropertyInfo? property, object? value)
        {

            if (property == null) return value ?? string.Empty;

            bool isSensitive = property.GetCustomAttributes(typeof(SensitiveAttribute), false).Any();
            return isSensitive ? "****" : value ?? string.Empty;
        }
    }
}
