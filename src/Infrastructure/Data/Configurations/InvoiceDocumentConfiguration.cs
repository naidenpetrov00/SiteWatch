using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class InvoiceDocumentConfiguration : IEntityTypeConfiguration<InvoiceDocument>
{
    public void Configure(EntityTypeBuilder<InvoiceDocument> builder)
    {
        builder.HasIndex(i => i.FileId).IsUnique();
        builder.HasIndex(i => i.SiteId);

        builder.Property(i => i.FileName)
            .HasMaxLength(512);

        builder.Property(i => i.ContentType)
            .HasMaxLength(128);

        builder.HasOne(i => i.Site)
            .WithMany(s => s.Invoices)
            .HasForeignKey(i => i.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

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
