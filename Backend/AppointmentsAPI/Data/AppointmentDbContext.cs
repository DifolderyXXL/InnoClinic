using AppointmentsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentsAPI.Data;

public class AppointmentDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentState> AppointmentStates => Set<AppointmentState>();

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AppointmentTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentStateMap());
    }
}

public class AppointmentTypeConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> entity)
    {        
        entity.HasKey(x => x.Id);
        
        entity .Property(b => b.ReservationId)
            .HasField("_reservationId");
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