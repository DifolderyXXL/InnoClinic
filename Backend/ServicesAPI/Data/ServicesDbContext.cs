using System;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ServicesAPI.Data.Configurations;
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
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}