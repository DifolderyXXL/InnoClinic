using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicesAPI.Models;

namespace ServicesAPI.Data.Configurations;

public class SpecializationTypeConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.HasIndex(x => x.SpecializationName).IsUnique();
    }
}