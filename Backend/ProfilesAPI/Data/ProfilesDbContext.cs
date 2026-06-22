using System;
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
    }
}

public class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasIndex(x => x.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PhoneNumber)
            .IsRequired();

        builder.Property(x => x.FirstName).IsRequired();
        builder.Property(x => x.LastName).IsRequired();
        builder.Property(x => x.MiddleName).IsRequired();

        builder.HasOne(x => x.Patient).WithOne(x => x.Account).HasForeignKey<Patient>(p => p.AccountId);
        builder.HasOne(x => x.Doctor).WithOne(x => x.Account).HasForeignKey<Doctor>(p => p.AccountId);
        builder.HasOne(x => x.Receptionist).WithOne(x => x.Account).HasForeignKey<Receptionist>(p => p.AccountId);
    }
}

public class PatientEntityTypeConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasIndex(x => x.Id);
    }
}
