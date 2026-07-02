using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicesAPI.Models;

namespace ServicesAPI.Data;

public class ServicesDbContext(DbContextOptions<ServicesDbContext> options) : DbContext(options)
{
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Specialization> Specializations => Set<Specialization>();

    public DbSet<ReservedTimeWindow> ReservedTimeWindows => Set<ReservedTimeWindow>();
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ServiceTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceCategoryTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SpecializationTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ReservedTimeWindowTypeConfiguration());
    }
}

public class ReservedTimeWindowTypeConfiguration : IEntityTypeConfiguration<ReservedTimeWindow>
{
    public void Configure(EntityTypeBuilder<ReservedTimeWindow> builder)
    {
        builder.HasIndex(x => new { x.Date, x.StartSlotIndex, x.SlotCount });
    }
}

public class ServiceTypeConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.HasIndex(x => x.ServiceName).IsUnique();

        builder.HasOne(x => x.ServiceCategory)
            .WithMany(c => c.Services)
            .HasForeignKey(x => x.CategoryId);

        builder.HasOne(x => x.Specialization)
            .WithMany(c => c.Services)
            .HasForeignKey(x => x.CategoryId);
    }
}

public class ServiceCategoryTypeConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.HasIndex(x => x.CategoryName).IsUnique();
    }
}

public class SpecializationTypeConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.HasIndex(x => x.SpecializationName).IsUnique();
    }
}