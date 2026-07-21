using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.Property(invoice => invoice.NumberId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEXT VALUE FOR [dbo].[InvoiceNumberIds]")
            .IsRequired();

        builder.Property(invoice => invoice.InvoiceNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(invoice => invoice.TaxIdentifier)
            .HasColumnName("Eik")
            .HasMaxLength(13)
            .IsRequired();

        builder.Property(invoice => invoice.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(invoice => invoice.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(invoice => invoice.PhoneNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(invoice => invoice.ContactPerson)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(invoice => invoice.PaymentTerm)
            .IsRequired();

        builder.Property(invoice => invoice.TotalValueExcludingVat)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(invoice => invoice.Vat)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(invoice => invoice.TotalValueIncludingVat)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(invoice => invoice.PaymentMethod)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(invoice => invoice.SupplierId);
        builder.HasIndex(invoice => invoice.InvoiceNumber);
        builder.HasIndex(invoice => invoice.Date);

        builder.HasOne(invoice => invoice.Supplier)
            .WithMany()
            .HasForeignKey(invoice => invoice.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
