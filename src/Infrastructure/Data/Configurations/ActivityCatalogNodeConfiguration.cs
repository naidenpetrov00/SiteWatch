using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class ActivityCatalogNodeConfiguration
    : IEntityTypeConfiguration<ActivityCatalogNode>
{
    public void Configure(EntityTypeBuilder<ActivityCatalogNode> builder)
    {
        builder.ToTable("ActivityCatalogNodes");

        builder.HasDiscriminator<string>("NodeType")
            .HasValue<ActivityFolder>("folder")
            .HasValue<Activity>("activity");

        builder.Property(node => node.Name)
            .HasMaxLength(ActivityCatalogNode.MaxNameLength)
            .IsRequired();
        builder.Property(node => node.NormalizedName)
            .HasMaxLength(ActivityCatalogNode.MaxNameLength)
            .IsRequired();
        builder.Property(node => node.SortOrder).IsRequired();

        builder.HasOne(node => node.ParentFolder)
            .WithMany(folder => folder.Children)
            .HasForeignKey(node => node.ParentFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(node => new { node.ParentFolderId, node.NormalizedName })
            .IsUnique()
            .HasFilter(null);
        builder.HasIndex(node => new { node.ParentFolderId, node.SortOrder });
    }
}
