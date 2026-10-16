using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class AssetCatalogItemConfiguration : IEntityTypeConfiguration<AssetCatalogItem>
{
    public void Configure(EntityTypeBuilder<AssetCatalogItem> builder)
    {
        builder.ToTable(
            "AssetCatalogItems",
            table => table.HasCheckConstraint(
                "CK_AssetCatalogItems_ReviewState",
                "([ReviewStatus] = N'PendingReview' AND [ReviewedById] IS NULL "
                + "AND [ReviewedAt] IS NULL AND [RejectionReason] IS NULL) OR "
                + "([ReviewStatus] = N'Active' AND [ReviewedById] IS NOT NULL "
                + "AND [ReviewedAt] IS NOT NULL AND [RejectionReason] IS NULL) OR "
                + "([ReviewStatus] = N'Rejected' AND [ReviewedById] IS NOT NULL "
                + "AND [ReviewedAt] IS NOT NULL)"));

        builder.Property(item => item.Classification)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(item => item.Name)
            .HasMaxLength(AssetCatalogItem.MaxNameLength)
            .IsRequired();
        builder.Property(item => item.Manufacturer)
            .HasMaxLength(AssetCatalogItem.MaxManufacturerLength);
        builder.Property(item => item.Model)
            .HasMaxLength(AssetCatalogItem.MaxModelLength);
        builder.Property(item => item.Description)
            .HasMaxLength(AssetCatalogItem.MaxDescriptionLength);
        builder.Property(item => item.ReviewStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(item => item.ReviewedById).HasMaxLength(450);
        builder.Property(item => item.RejectionReason)
            .HasMaxLength(AssetCatalogItem.MaxRejectionReasonLength);
        builder.Property(item => item.RowVersion)
            .IsRowVersion();

        builder.HasIndex(item => item.Classification);
        builder.HasIndex(item => item.Name);
        builder.HasIndex(item => item.ReviewStatus);

        builder.HasOne(item => item.ReviewedBy)
            .WithMany()
            .HasForeignKey(item => item.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(item => item.Assets)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
