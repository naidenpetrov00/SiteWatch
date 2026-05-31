using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");

        builder.Property(i => i.ProductCode)
            .HasMaxLength(128);

        builder.Property(i => i.ProductName)
            .HasMaxLength(512);

        builder.Property(i => i.Unit)
            .HasMaxLength(64);

        builder.Property(i => i.Quantity)
            .HasPrecision(18, 6);

        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(i => i.Discount)
            .HasPrecision(18, 2);

        builder.Property(i => i.VatRate)
            .HasPrecision(9, 6);

        builder.Property(i => i.LineTotal)
            .HasPrecision(18, 2);

        builder.Property(i => i.Confidence)
            .HasPrecision(9, 6);
    }
}
