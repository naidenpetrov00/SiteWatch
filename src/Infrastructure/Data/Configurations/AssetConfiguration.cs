using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable(
            "Assets",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Assets_HandoverState",
                    "([OperationalStatus] = N'InTransfer' "
                    + "AND [ResponsibleWorkerId] IS NOT NULL "
                    + "AND [PendingHandoverRecipientId] IS NOT NULL "
                    + "AND [PreHandoverOperationalStatus] IS NOT NULL "
                    + "AND [PreHandoverOperationalStatus] NOT IN "
                    + "(N'InTransfer', N'Missing', N'Retired')) OR "
                    + "([OperationalStatus] <> N'InTransfer' "
                    + "AND [PendingHandoverRecipientId] IS NULL "
                    + "AND [PreHandoverOperationalStatus] IS NULL)");
                table.HasCheckConstraint(
                    "CK_Assets_HandoverRecipient",
                    "[PendingHandoverRecipientId] IS NULL "
                    + "OR [PendingHandoverRecipientId] <> [ResponsibleWorkerId]");
            });

        builder.Ignore(asset => asset.AssetCode);

        builder.Property(asset => asset.NumberId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEXT VALUE FOR [dbo].[AssetNumberIds]")
            .IsRequired();
        builder.Property(asset => asset.QrIdentifier).IsRequired();
        builder.Property(asset => asset.ManufacturerSerialNumber)
            .HasMaxLength(Asset.MaxManufacturerSerialNumberLength);
        builder.Property(asset => asset.ResponsibleWorkerId).HasMaxLength(450);
        builder.Property(asset => asset.PendingHandoverRecipientId).HasMaxLength(450);
        builder.Property(asset => asset.Condition)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(asset => asset.OperationalStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(asset => asset.PreHandoverOperationalStatus)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(asset => asset.ReviewStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(asset => asset.HistorySequence).IsRequired();
        builder.Property(asset => asset.RowVersion).IsRowVersion();

        builder.HasIndex(asset => asset.NumberId).IsUnique();
        builder.HasIndex(asset => asset.QrIdentifier).IsUnique();
        builder.HasIndex(asset => asset.CatalogItemId);
        builder.HasIndex(asset => asset.SiteId);
        builder.HasIndex(asset => asset.ResponsibleWorkerId);
        builder.HasIndex(asset => asset.ManufacturerSerialNumber);
        builder.HasIndex(asset => asset.Condition);
        builder.HasIndex(asset => asset.OperationalStatus);
        builder.HasIndex(asset => asset.ReviewStatus);
        builder.HasIndex(asset => new
        {
            asset.SiteId,
            asset.OperationalStatus,
            asset.NumberId
        });

        builder.HasOne(asset => asset.CatalogItem)
            .WithMany(item => item.Assets)
            .HasForeignKey(asset => asset.CatalogItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(asset => asset.Site)
            .WithMany()
            .HasForeignKey(asset => asset.SiteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(asset => asset.ResponsibleWorker)
            .WithMany()
            .HasForeignKey(asset => asset.ResponsibleWorkerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(asset => asset.PendingHandoverRecipient)
            .WithMany()
            .HasForeignKey(asset => asset.PendingHandoverRecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(asset => asset.AssetEvents)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
