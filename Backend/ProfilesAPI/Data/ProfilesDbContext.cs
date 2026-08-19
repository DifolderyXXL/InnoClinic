using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfilesAPI.Models;

namespace ProfilesAPI.Data;

public class ProfilesDbContext(DbContextOptions<ProfilesDbContext> options)
    : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Receptionist> Receptionists => Set<Receptionist>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Specialization> Specializations => Set<Specialization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AccountEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PatientEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SpecializationEntityTypeConfiguration());
        
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}

public class SpecializationEntityTypeConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever(); 
    }
}

public class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(x => x.Id)
            .IsClustered(false);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PhoneNumber)
            .IsRequired();

        builder.Property(x => x.FirstName).IsRequired();
        builder.Property(x => x.LastName).IsRequired();

        builder.HasOne(x => x.Patient).WithOne(x => x.Account).HasForeignKey<Patient>(p => p.AccountId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Doctor).WithOne(x => x.Account).HasForeignKey<Doctor>(p => p.AccountId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Receptionist).WithOne(x => x.Account).HasForeignKey<Receptionist>(p => p.AccountId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PatientEntityTypeConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(x => x.Id);
    }
}

public class DoctorEntityTypeConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Specialization)
            .WithMany()
            .HasForeignKey(x => x.SpecializationId);
    }
}
