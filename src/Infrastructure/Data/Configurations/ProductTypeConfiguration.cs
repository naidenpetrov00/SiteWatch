using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("ProductTypes");
        builder.Property(productType => productType.Name)
            .HasMaxLength(ProductType.MaxNameLength)
            .IsRequired();
        builder.Property(productType => productType.NormalizedName)
            .HasMaxLength(ProductType.MaxNameLength)
            .IsRequired();
        builder.HasOne(productType => productType.Category)
            .WithMany()
            .HasForeignKey(productType => productType.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(productType => new
        {
            productType.CategoryId,
            productType.NormalizedName
        }).IsUnique();
    }
}
