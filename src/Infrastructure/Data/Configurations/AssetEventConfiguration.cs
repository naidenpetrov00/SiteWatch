using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class AssetEventConfiguration : IEntityTypeConfiguration<AssetEvent>
{
    public void Configure(EntityTypeBuilder<AssetEvent> builder)
    {
        builder.ToTable("AssetEvents");

        builder.Property(assetEvent => assetEvent.SequenceNumber).IsRequired();
        builder.Property(assetEvent => assetEvent.EventType)
            .HasConversion<string>()
            .HasMaxLength(48)
            .IsRequired();
        builder.Property(assetEvent => assetEvent.ActorId)
            .HasMaxLength(450)
            .IsRequired();
        builder.Property(assetEvent => assetEvent.OccurredAt).IsRequired();
        builder.Property(assetEvent => assetEvent.Note)
            .HasMaxLength(AssetEvent.MaxNoteLength);

        ConfigurePreviousState(builder);
        ConfigureResultingState(builder);

        builder.HasIndex(assetEvent => new
        {
            assetEvent.AssetId,
            assetEvent.SequenceNumber
        }).IsUnique();
        builder.HasIndex(assetEvent => new
        {
            assetEvent.AssetId,
            assetEvent.OccurredAt,
            assetEvent.SequenceNumber
        });
        builder.HasIndex(assetEvent => assetEvent.ActorId);

        builder.HasOne(assetEvent => assetEvent.Asset)
            .WithMany(asset => asset.AssetEvents)
            .HasForeignKey(assetEvent => assetEvent.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(assetEvent => assetEvent.Actor)
            .WithMany()
            .HasForeignKey(assetEvent => assetEvent.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        ConfigureSnapshotRelationships(builder);
    }

    private static void ConfigurePreviousState(EntityTypeBuilder<AssetEvent> builder)
    {
        builder.Property(assetEvent => assetEvent.PreviousClassification)
            .HasConversion<string>()
            .HasMaxLength(16);
        builder.Property(assetEvent => assetEvent.PreviousName)
            .HasMaxLength(Asset.MaxNameLength);
        builder.Property(assetEvent => assetEvent.PreviousManufacturer)
            .HasMaxLength(Asset.MaxManufacturerLength);
        builder.Property(assetEvent => assetEvent.PreviousModel)
            .HasMaxLength(Asset.MaxModelLength);
        builder.Property(assetEvent => assetEvent.PreviousDescription)
            .HasMaxLength(Asset.MaxDescriptionLength);
        builder.Property(assetEvent => assetEvent.PreviousManufacturerSerialNumber)
            .HasMaxLength(Asset.MaxManufacturerSerialNumberLength);
        builder.Property(assetEvent => assetEvent.PreviousResponsibleWorkerId).HasMaxLength(450);
        builder.Property(assetEvent => assetEvent.PreviousPendingHandoverRecipientId)
            .HasMaxLength(450);
        builder.Property(assetEvent => assetEvent.PreviousCondition)
            .HasConversion<string>()
            .HasMaxLength(16);
        builder.Property(assetEvent => assetEvent.PreviousOperationalStatus)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(assetEvent => assetEvent.PreviousPreHandoverOperationalStatus)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(assetEvent => assetEvent.PreviousReviewStatus)
            .HasConversion<string>()
            .HasMaxLength(32);
    }

    private static void ConfigureResultingState(EntityTypeBuilder<AssetEvent> builder)
    {
        builder.Property(assetEvent => assetEvent.ResultingClassification)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(assetEvent => assetEvent.ResultingName)
            .HasMaxLength(Asset.MaxNameLength)
            .IsRequired();
        builder.Property(assetEvent => assetEvent.ResultingManufacturer)
            .HasMaxLength(Asset.MaxManufacturerLength);
        builder.Property(assetEvent => assetEvent.ResultingModel)
            .HasMaxLength(Asset.MaxModelLength);
        builder.Property(assetEvent => assetEvent.ResultingDescription)
            .HasMaxLength(Asset.MaxDescriptionLength);
        builder.Property(assetEvent => assetEvent.ResultingQrIdentifier).IsRequired();
        builder.Property(assetEvent => assetEvent.ResultingManufacturerSerialNumber)
            .HasMaxLength(Asset.MaxManufacturerSerialNumberLength);
        builder.Property(assetEvent => assetEvent.ResultingSiteId).IsRequired();
        builder.Property(assetEvent => assetEvent.ResultingResponsibleWorkerId).HasMaxLength(450);
        builder.Property(assetEvent => assetEvent.ResultingPendingHandoverRecipientId)
            .HasMaxLength(450);
        builder.Property(assetEvent => assetEvent.ResultingCondition)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(assetEvent => assetEvent.ResultingOperationalStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(assetEvent => assetEvent.ResultingPreHandoverOperationalStatus)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(assetEvent => assetEvent.ResultingReviewStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
    }

    private static void ConfigureSnapshotRelationships(EntityTypeBuilder<AssetEvent> builder)
    {
        builder.HasOne<Site>()
            .WithMany()
            .HasForeignKey(assetEvent => assetEvent.PreviousSiteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Site>()
            .WithMany()
            .HasForeignKey(assetEvent => assetEvent.ResultingSiteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(assetEvent => assetEvent.PreviousResponsibleWorkerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(assetEvent => assetEvent.ResultingResponsibleWorkerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(assetEvent => assetEvent.PreviousPendingHandoverRecipientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(assetEvent => assetEvent.ResultingPendingHandoverRecipientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
