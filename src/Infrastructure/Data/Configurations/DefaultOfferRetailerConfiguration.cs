using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class DefaultOfferRetailerConfiguration
    : IEntityTypeConfiguration<DefaultOfferRetailer>
{
    public void Configure(EntityTypeBuilder<DefaultOfferRetailer> builder)
    {
        builder.ToTable(
            "DefaultOfferRetailers",
            table => table.HasCheckConstraint(
                "CK_DefaultOfferRetailers_SortOrder",
                $"[SortOrder] >= 0 AND [SortOrder] < {DefaultOfferRetailer.MaximumCount}"));

        builder.Property(item => item.SortOrder).IsRequired();

        builder.HasIndex(item => item.RetailerId).IsUnique();
        builder.HasIndex(item => item.SortOrder).IsUnique();

        builder.HasOne(item => item.Retailer)
            .WithMany()
            .HasForeignKey(item => item.RetailerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
