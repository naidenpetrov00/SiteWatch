using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class InvoiceReviewIssueConfiguration : IEntityTypeConfiguration<InvoiceReviewIssue>
{
    public void Configure(EntityTypeBuilder<InvoiceReviewIssue> builder)
    {
        builder.ToTable("InvoiceReviewIssues");

        builder.Property(i => i.FieldPath)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(i => i.ExtractedValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.Reason)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(i => i.Confidence)
            .HasPrecision(9, 6);

        builder.Property(i => i.CorrectedValue)
            .HasColumnType("nvarchar(max)");
    }
}
