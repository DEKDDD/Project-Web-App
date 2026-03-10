using Microsoft.EntityFrameworkCore;
using WebApplicationASP.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Activity> Activities { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    // ตั้งค่าให้ User เก็บ List เป็นคอลัมน์เดียว
    modelBuilder.Entity<User>(entity =>
    {
        entity.Property(u => u.CreatedActivityIds);
        entity.Property(u => u.JoinedActivityIds);
    });
    }
}

