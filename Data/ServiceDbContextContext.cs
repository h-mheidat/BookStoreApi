using Microsoft.EntityFrameworkCore;
using BookStoreApi.Models;

namespace BookStoreApi.Data
{
    public class ServiceDbContextContext : AuditableDbContext
    {
        public ServiceDbContextContext(DbContextOptions<ServiceDbContextContext> options, 
                           IHttpContextAccessor httpContextAccessor, 
                           ILogger<AuditableDbContext> logger)
            : base(options, httpContextAccessor, logger)
        {}

        // DbSets for your entities
        public DbSet<Book> Books { get; set; }
        public DbSet<Car> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(e => e.Details)
                    .HasColumnType("json");
            });

            // Configure your entities
            modelBuilder.Entity<Book>().ToTable("Books");
            modelBuilder.Entity<Car>().ToTable("Cars");
        }
    }
}
