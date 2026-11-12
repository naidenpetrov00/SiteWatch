using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class ProductBrandConfiguration : IEntityTypeConfiguration<ProductBrand>
{
    public void Configure(EntityTypeBuilder<ProductBrand> builder)
    {
        builder.ToTable("ProductBrands");
        builder.Property(brand => brand.Name)
            .HasMaxLength(ProductBrand.MaxNameLength)
            .IsRequired();
        builder.Property(brand => brand.NormalizedName)
            .HasMaxLength(ProductBrand.MaxNameLength)
            .IsRequired();
        builder.HasIndex(brand => brand.NormalizedName).IsUnique();
    }
}
