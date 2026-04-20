using Microsoft.EntityFrameworkCore;
using task.Models;

namespace task.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CityEntity> Cities => Set<CityEntity>();
    public DbSet<OfficeEntity> Offices => Set<OfficeEntity>();
    public DbSet<PhoneEntity> Phones => Set<PhoneEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CityEntity>(e =>
        {
            e.HasIndex(c => c.Name).HasDatabaseName("ix_cities_name");
            e.HasIndex(c => c.Code).HasDatabaseName("ix_cities_code");
            e.HasIndex(c => c.CityId).HasDatabaseName("ix_cities_city_id");
        });

        modelBuilder.Entity<OfficeEntity>(e =>
        {
            e.HasIndex(o => o.CityId).HasDatabaseName("ix_offices_city_id");
            e.HasIndex(o => o.CityCode).HasDatabaseName("ix_offices_city_code");
            e.Property(o => o.OfficeType).HasConversion<string>();
        });

        modelBuilder.Entity<PhoneEntity>(e =>
        {
            e.HasIndex(p => p.OfficeId).HasDatabaseName("ix_phones_office_id");
        });
    }
}
