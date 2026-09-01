using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class IssueAttachmentConfiguration : IEntityTypeConfiguration<IssueAttachment>
{
    public void Configure(EntityTypeBuilder<IssueAttachment> builder)
    {
        builder.Property(attachment => attachment.FileName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(attachment => attachment.ContentType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(attachment => attachment.Kind)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(attachment => new { attachment.IssueId, attachment.Created });

        builder.HasOne(attachment => attachment.Issue)
            .WithMany(issue => issue.Attachments)
            .HasForeignKey(attachment => attachment.IssueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
