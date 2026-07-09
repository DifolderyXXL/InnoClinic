using AppointmentsAPI.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentsAPI.Data;

public class AppointmentDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentState> AppointmentStates => Set<AppointmentState>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new DoctorTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentStateMap());
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}

public class DoctorTypeConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> entity)
    {        
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.AccountId);

        entity.HasMany(x => x.Appointments)
            .WithOne(x => x.Doctor)
            .HasForeignKey(x => x.DoctorId);
    }
}

public class AppointmentTypeConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> entity)
    {        
        entity.HasKey(x => x.Id);
    }
}

public class AppointmentStateMap : IEntityTypeConfiguration<AppointmentState>
{
    public void Configure(EntityTypeBuilder<AppointmentState> entity)
    {        
        entity.HasKey(x => x.CorrelationId);
        
        entity.Property(x => x.CurrentState).HasMaxLength(64);
    }
}