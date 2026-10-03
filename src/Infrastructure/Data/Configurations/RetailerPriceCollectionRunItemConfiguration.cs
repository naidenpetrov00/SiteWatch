using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RetailerPriceCollectionRunItemConfiguration
    : IEntityTypeConfiguration<RetailerPriceCollectionRunItem>
{
    public void Configure(EntityTypeBuilder<RetailerPriceCollectionRunItem> builder)
    {
        builder.ToTable(
            "RetailerPriceCollectionRunItems",
            table => table.HasCheckConstraint(
                "CK_RetailerPriceCollectionRunItems_Lifecycle",
                "([Status] = N'queued' AND [LeaseToken] IS NULL AND [LeaseExpiresAt] IS NULL "
                + "AND [CompletedAt] IS NULL AND [RetailerPriceObservationId] IS NULL) OR "
                + "([Status] = N'running' AND [LeaseToken] IS NOT NULL AND [LeaseExpiresAt] IS NOT NULL "
                + "AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NULL "
                + "AND [RetailerPriceObservationId] IS NULL) OR "
                + "([Status] = N'succeeded' AND [LeaseToken] IS NULL AND [LeaseExpiresAt] IS NULL "
                + "AND [CompletedAt] IS NOT NULL AND [RetailerPriceObservationId] IS NOT NULL "
                + "AND [FinalSourceUrl] IS NOT NULL AND [DiagnosticCode] IS NULL) OR "
                + "([Status] IN (N'failed', N'skipped') AND [LeaseToken] IS NULL "
                + "AND [LeaseExpiresAt] IS NULL AND [CompletedAt] IS NOT NULL "
                + "AND [RetailerPriceObservationId] IS NULL AND [DiagnosticCode] IS NOT NULL "
                + "AND [DiagnosticMessage] IS NOT NULL)")
        );

        builder.Property(item => item.Status)
            .HasConversion(
                status => status.ToCode(),
                value => ParseItemStatus(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(item => item.CapturedProductUrl)
            .HasMaxLength(RetailerListing.MaxProductUrlLength);
        builder.Property(item => item.FinalSourceUrl)
            .HasMaxLength(RetailerListing.MaxProductUrlLength);
        builder.Property(item => item.DiagnosticCode)
            .HasMaxLength(RetailerPriceCollectionRunItem.MaxDiagnosticCodeLength);
        builder.Property(item => item.DiagnosticMessage)
            .HasMaxLength(RetailerPriceCollectionRunItem.MaxDiagnosticMessageLength);
        builder.Property(item => item.RowVersion).IsRowVersion();

        builder.HasOne(item => item.Run)
            .WithMany(run => run.Items)
            .HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.RetailerListing)
            .WithMany()
            .HasForeignKey(item => item.RetailerListingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.RetailerPriceObservation)
            .WithMany()
            .HasForeignKey(item => item.RetailerPriceObservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => new { item.RunId, item.RetailerListingId }).IsUnique();
        builder.HasIndex(item => new { item.Status, item.LeaseExpiresAt, item.Id });
        builder.HasIndex(item => item.RetailerPriceObservationId)
            .IsUnique()
            .HasFilter("[RetailerPriceObservationId] IS NOT NULL");
    }

    private static RetailerPriceCollectionRunItemStatus ParseItemStatus(string value) => value switch
    {
        "queued" => RetailerPriceCollectionRunItemStatus.Queued,
        "running" => RetailerPriceCollectionRunItemStatus.Running,
        "succeeded" => RetailerPriceCollectionRunItemStatus.Succeeded,
        "failed" => RetailerPriceCollectionRunItemStatus.Failed,
        "skipped" => RetailerPriceCollectionRunItemStatus.Skipped,
        _ => throw new InvalidOperationException($"Unsupported collection-item status '{value}'.")
    };
}
