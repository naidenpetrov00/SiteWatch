using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.Property(activity => activity.NumberId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEXT VALUE FOR [dbo].[ActivityNumberIds]")
            .IsRequired();
        builder.Property(activity => activity.Description)
            .HasMaxLength(Activity.MaxDescriptionLength);
        builder.Property(activity => activity.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(activity => activity.NumberId)
            .IsUnique()
            .HasFilter("[NumberId] IS NOT NULL");
        builder.HasIndex(activity => activity.Status);
    }
}
