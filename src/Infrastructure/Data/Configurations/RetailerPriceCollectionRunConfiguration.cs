using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RetailerPriceCollectionRunConfiguration
    : IEntityTypeConfiguration<RetailerPriceCollectionRun>
{
    public void Configure(EntityTypeBuilder<RetailerPriceCollectionRun> builder)
    {
        builder.ToTable(
            "RetailerPriceCollectionRuns",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_RetailerPriceCollectionRuns_Counters",
                    "[TotalCount] >= 0 AND [QueuedCount] >= 0 AND [RunningCount] >= 0 "
                    + "AND [ProcessedCount] >= 0 AND [SucceededCount] >= 0 "
                    + "AND [FailedCount] >= 0 AND [SkippedCount] >= 0 "
                    + "AND [ProcessedCount] = [SucceededCount] + [FailedCount] + [SkippedCount] "
                    + "AND [TotalCount] = [QueuedCount] + [RunningCount] + [ProcessedCount]");
                table.HasCheckConstraint(
                    "CK_RetailerPriceCollectionRuns_Lifecycle",
                    "([Status] = N'queued' AND [StartedAt] IS NULL AND [CompletedAt] IS NULL) OR "
                    + "([Status] = N'running' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NULL) OR "
                    + "([Status] = N'completed' AND [QueuedCount] = 0 AND [RunningCount] = 0 "
                    + "AND [FailedCount] = 0 AND [CompletedAt] IS NOT NULL) OR "
                    + "([Status] = N'completedWithFailures' AND [QueuedCount] = 0 "
                    + "AND [RunningCount] = 0 AND [FailedCount] > 0 AND [CompletedAt] IS NOT NULL)");
            });

        builder.Property(run => run.Status)
            .HasConversion(
                status => status.ToCode(),
                value => ParseRunStatus(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(run => run.RequestedBy).HasMaxLength(450).IsRequired();

        builder.HasOne(run => run.CompanyPerson)
            .WithMany()
            .HasForeignKey(run => run.CompanyPersonId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(run => run.ExtractionProfile)
            .WithMany()
            .HasForeignKey(run => run.ExtractionProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(run => run.Offer)
            .WithMany()
            .HasForeignKey(run => run.OfferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(run => new { run.CompanyPersonId, run.RequestedAt, run.Id })
            .IsDescending(false, true, true);
        builder.HasIndex(run => new { run.OfferId, run.RequestedAt, run.Id })
            .IsDescending(false, true, true)
            .HasFilter("[OfferId] IS NOT NULL");
        builder.HasIndex(run => run.CompanyPersonId)
            .IsUnique()
            .HasDatabaseName("IX_RetailerPriceCollectionRuns_OneUnfinishedPerCompany")
            .HasFilter("[Status] IN (N'queued', N'running')");
        builder.Navigation(run => run.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static RetailerPriceCollectionRunStatus ParseRunStatus(string value) => value switch
    {
        "queued" => RetailerPriceCollectionRunStatus.Queued,
        "running" => RetailerPriceCollectionRunStatus.Running,
        "completed" => RetailerPriceCollectionRunStatus.Completed,
        "completedWithFailures" => RetailerPriceCollectionRunStatus.CompletedWithFailures,
        _ => throw new InvalidOperationException($"Unsupported collection-run status '{value}'.")
    };
}
