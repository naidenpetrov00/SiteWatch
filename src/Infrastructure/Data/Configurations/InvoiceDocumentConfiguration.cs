using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class InvoiceDocumentConfiguration : IEntityTypeConfiguration<InvoiceDocument>
{
    public void Configure(EntityTypeBuilder<InvoiceDocument> builder)
    {
        builder.ToTable("InvoiceDocuments");

        builder.HasIndex(i => i.SiteId);
        builder.HasIndex(i => i.StoredFilePath).IsUnique();

        builder.HasOne<Site>()
            .WithMany()
            .HasForeignKey(i => i.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.OriginalFileName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(i => i.StoredFilePath)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(i => i.SupplierName)
            .HasMaxLength(256);

        builder.Property(i => i.SupplierEik)
            .HasMaxLength(32);

        builder.Property(i => i.SupplierVatNumber)
            .HasMaxLength(32);

        builder.Property(i => i.BuyerName)
            .HasMaxLength(256);

        builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(128);

        builder.Property(i => i.Currency)
            .HasMaxLength(8);

        builder.Property(i => i.RawExtractionJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.RawOcrText)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.DocumentType)
            .HasConversion<int>();

        builder.Property(i => i.Status)
            .HasConversion<int>();

        builder.Property(i => i.NetTotal)
            .HasPrecision(18, 2);

        builder.Property(i => i.NetTotalBgn)
            .HasPrecision(18, 2);

        builder.Property(i => i.NetTotalEur)
            .HasPrecision(18, 2);

        builder.Property(i => i.VatTotal)
            .HasPrecision(18, 2);

        builder.Property(i => i.VatTotalBgn)
            .HasPrecision(18, 2);

        builder.Property(i => i.VatTotalEur)
            .HasPrecision(18, 2);

        builder.Property(i => i.GrossTotal)
            .HasPrecision(18, 2);

        builder.Property(i => i.GrossTotalBgn)
            .HasPrecision(18, 2);

        builder.Property(i => i.GrossTotalEur)
            .HasPrecision(18, 2);

        builder.Property(i => i.OverallConfidence)
            .HasPrecision(9, 6);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.HasMany(i => i.Lines)
            .WithOne(il => il.InvoiceDocument)
            .HasForeignKey(il => il.InvoiceDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.ReviewIssues)
            .WithOne(ri => ri.InvoiceDocument)
            .HasForeignKey(ri => ri.InvoiceDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
