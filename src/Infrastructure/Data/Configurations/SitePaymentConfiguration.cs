using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class SitePaymentConfiguration : IEntityTypeConfiguration<SitePayment>
{
    public void Configure(EntityTypeBuilder<SitePayment> builder)
    {
        builder.ToTable("SitePayments");

        builder.Property(sitePayment => sitePayment.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sitePayment => sitePayment.Direction)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();

        builder.HasIndex(sitePayment => new { sitePayment.InvoiceId, sitePayment.SiteId })
            .IsUnique();

        builder.HasOne(sitePayment => sitePayment.Invoice)
            .WithMany(invoice => invoice.SitePayments)
            .HasForeignKey(sitePayment => sitePayment.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sitePayment => sitePayment.Site)
            .WithMany(site => site.Payments)
            .HasForeignKey(sitePayment => sitePayment.SiteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
