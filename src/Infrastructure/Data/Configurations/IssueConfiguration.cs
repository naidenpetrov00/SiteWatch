using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.Property(issue => issue.NumberId)
            .HasDefaultValueSql("NEXT VALUE FOR [dbo].[IssueNumberIds]");

        builder.Property(issue => issue.Title)
            .HasMaxLength(200);

        builder.Property(issue => issue.Description)
            .HasMaxLength(4000);

        builder.Property(issue => issue.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(issue => issue.NumberId).IsUnique();
        builder.HasIndex(issue => issue.SiteId);
        builder.HasIndex(issue => issue.Status);
        builder.HasIndex(issue => issue.StartDate);
        builder.HasIndex(issue => issue.EndDate);

        builder.HasOne(issue => issue.Site)
            .WithMany(site => site.Issues)
            .HasForeignKey(issue => issue.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(issue => issue.AssignedWorkers)
            .WithMany(user => user.AssignedIssues)
            .UsingEntity(join => join.ToTable("IssueWorkers"));
    }
}
