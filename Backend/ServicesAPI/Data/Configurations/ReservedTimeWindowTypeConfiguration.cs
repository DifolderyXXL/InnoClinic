using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicesAPI.Models;

namespace ServicesAPI.Data.Configurations;

public class ReservedTimeWindowTypeConfiguration : IEntityTypeConfiguration<ReservedTimeWindow>
{
    public void Configure(EntityTypeBuilder<ReservedTimeWindow> builder)
    {
        builder.HasIndex(x => new { x.DoctorId, x.Date });
        builder.HasIndex(x => new { x.PatientId, x.Date });
    }
}