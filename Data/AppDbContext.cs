using Microsoft.EntityFrameworkCore;
using EquipmentManagement.Models;

namespace EquipmentManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>()
            .HasIndex(x => x.ManagementNumber)
            .IsUnique();
    }
}