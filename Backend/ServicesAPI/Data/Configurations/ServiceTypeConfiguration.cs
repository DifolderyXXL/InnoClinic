using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicesAPI.Models;

namespace ServicesAPI.Data.Configurations;

public class ServiceTypeConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.HasIndex(x => x.ServiceName).IsUnique();

        builder.HasOne(x => x.ServiceCategory)
            .WithMany(c => c.Services)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Specialization)
            .WithMany(c => c.Services)
            .HasForeignKey(x => x.SpecializationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}