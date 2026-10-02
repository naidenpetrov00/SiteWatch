using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class OfferActivityConfiguration : IEntityTypeConfiguration<OfferActivity>
{
    public void Configure(EntityTypeBuilder<OfferActivity> builder)
    {
        builder.ToTable("OfferActivities");

        builder.Property(activity => activity.ActivityNumberId).IsRequired();
        builder.Property(activity => activity.Name)
            .HasMaxLength(ActivityCatalogNode.MaxNameLength)
            .IsRequired();
        builder.Property(activity => activity.Description)
            .HasMaxLength(Activity.MaxDescriptionLength);
        builder.Property(activity => activity.SortOrder).IsRequired();

        builder.HasOne(activity => activity.Offer)
            .WithMany(offer => offer.Activities)
            .HasForeignKey(activity => activity.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(activity => activity.SourceActivity)
            .WithMany()
            .HasForeignKey(activity => activity.SourceActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(activity => new { activity.OfferId, activity.SourceActivityId })
            .IsUnique();
        builder.HasIndex(activity => new { activity.OfferId, activity.SortOrder });
    }
}
