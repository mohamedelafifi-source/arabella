using Microsoft.EntityFrameworkCore;
using arabella.Models;

namespace arabella.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Phone> Phones => Set<Phone>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Infraction> Infractions => Set<Infraction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Unit>(e =>
        {
            e.HasKey(u => u.UnitNumber);
            e.Property(u => u.UnitNumber).HasMaxLength(10);
            e.Property(u => u.OwnerName).HasMaxLength(40);
            e.Property(u => u.OwnerId).HasMaxLength(20);
            e.Property(u => u.OwnerMailAddress).HasMaxLength(30);
            e.Property(u => u.UserName).HasMaxLength(40);
            e.Property(u => u.UserId).HasMaxLength(20);
            e.Property(u => u.UserMailAddress).HasMaxLength(30);
            e.Property(u => u.SpouseName).HasMaxLength(40);
            e.Property(u => u.SpouseId).HasMaxLength(20);
            e.Property(u => u.Experience).HasMaxLength(40);
        });

        modelBuilder.Entity<Phone>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.UnitNumber).HasMaxLength(10);
            e.Property(p => p.Telephone).HasMaxLength(15);
            e.HasOne(p => p.Unit).WithMany(u => u.Phones).HasForeignKey(p => p.UnitNumber).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Car>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.UnitNumber).HasMaxLength(10);
            e.Property(c => c.CarModel).HasMaxLength(20);
            e.Property(c => c.CarNumber).HasMaxLength(8);
            e.HasOne(c => c.Unit).WithMany(u => u.Cars).HasForeignKey(c => c.UnitNumber).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Child>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.UnitNumber).HasMaxLength(10);
            e.Property(c => c.Name).HasMaxLength(40);
            e.Property(c => c.Telephone).HasMaxLength(15);
            e.HasOne(c => c.Unit).WithMany(u => u.Children).HasForeignKey(c => c.UnitNumber).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Pet>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.UnitNumber).HasMaxLength(10);
            e.Property(p => p.Type).HasMaxLength(30);
            e.Property(p => p.Size).HasMaxLength(20);
            e.Property(p => p.Color).HasMaxLength(50);
            e.Property(p => p.PetId).HasMaxLength(10);
            e.HasOne(p => p.Unit).WithMany(u => u.Pets).HasForeignKey(p => p.UnitNumber).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Infraction>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.UnitNumber).HasMaxLength(10);
            e.Property(i => i.Description).HasMaxLength(20);
            e.HasOne(i => i.Unit).WithMany(u => u.Infractions).HasForeignKey(i => i.UnitNumber).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
