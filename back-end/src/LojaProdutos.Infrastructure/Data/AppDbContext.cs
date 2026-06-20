using LojaProdutos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LojaProdutos.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryLog> CategoryLogs { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DateCreate).IsRequired();
            entity.Property(e => e.Tags).HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Categories)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<CategoryLog>(entity =>
        {
            entity.ToTable("CategoryLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
            entity.Property(e => e.User).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.Category).WithMany(e => e.Logs).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DateCreate).IsRequired();
            entity.Property(e => e.Tags).HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Informática" },
            new Department { Id = 2, Name = "Logística" }
        );
    }
}
