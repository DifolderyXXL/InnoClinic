using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicesAPI.Models;

namespace ServicesAPI.Data.Configurations;

public class ServiceCategoryTypeConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.HasIndex(x => x.CategoryName).IsUnique();
    }
}