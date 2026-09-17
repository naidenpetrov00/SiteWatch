using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RetailerConfiguration : IEntityTypeConfiguration<Retailer>
{
    public void Configure(EntityTypeBuilder<Retailer> builder)
    {
        builder.ToTable("Retailers");

        builder.Property(retailer => retailer.DisplayName)
            .HasMaxLength(Retailer.MaxDisplayNameLength)
            .IsRequired();
        builder.Property(retailer => retailer.NormalizedName)
            .HasMaxLength(Retailer.MaxNormalizedNameLength)
            .IsRequired();
        builder.Property(retailer => retailer.BaseWebsiteUrl)
            .HasMaxLength(RetailerWebsite.MaxBaseUrlLength)
            .IsRequired();
        builder.Property(retailer => retailer.NormalizedWebsiteHost)
            .HasMaxLength(RetailerWebsite.MaxHostLength)
            .IsRequired();
        builder.Property(retailer => retailer.Notes)
            .HasMaxLength(Retailer.MaxNotesLength);
        builder.Property(retailer => retailer.IsActive).IsRequired();

        builder.HasIndex(retailer => retailer.NormalizedName).IsUnique();
        builder.HasIndex(retailer => retailer.NormalizedWebsiteHost).IsUnique();
        builder.HasIndex(retailer => retailer.IsActive);

        builder.HasOne(retailer => retailer.CompanyPerson)
            .WithMany()
            .HasForeignKey(retailer => retailer.CompanyPersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
