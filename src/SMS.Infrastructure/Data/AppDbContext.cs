using SMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SMS.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for the Student Management System.
/// Configures entity mappings, constraints, and default values.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Students table.</summary>
    public DbSet<Student> Students => Set<Student>();

    /// <summary>Users table (for JWT authentication).</summary>
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Student configuration ──────────────────────────────────────
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(s => s.Email)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.HasIndex(s => s.Email)
                  .IsUnique();

            entity.Property(s => s.Course)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(s => s.CreatedDate)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Age constraint: 1 – 120
            entity.ToTable(t => t.HasCheckConstraint("CK_Students_Age", "[Age] > 0 AND [Age] < 120"));
        });

        // ── User configuration ────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Username)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasIndex(u => u.Username)
                  .IsUnique();

            entity.Property(u => u.PasswordHash)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(u => u.Role)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("Admin");

            entity.Property(u => u.CreatedDate)
                  .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
