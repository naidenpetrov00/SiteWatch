using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers");

        builder.Property(offer => offer.NumberId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEXT VALUE FOR [dbo].[OfferNumberIds]")
            .IsRequired();
        builder.Property(offer => offer.Title)
            .HasMaxLength(Offer.MaxTitleLength);
        builder.Property(offer => offer.Notes)
            .HasMaxLength(Offer.MaxNotesLength);
        builder.Property(offer => offer.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(offer => offer.NumberId).IsUnique();
        builder.HasIndex(offer => offer.SiteId);
        builder.HasIndex(offer => new { offer.SiteId, offer.Status, offer.NumberId });

        builder.HasOne(offer => offer.Site)
            .WithMany(site => site.Offers)
            .HasForeignKey(offer => offer.SiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
