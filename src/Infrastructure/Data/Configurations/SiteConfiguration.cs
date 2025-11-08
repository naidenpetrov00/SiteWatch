using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.HasMany(site => site.Users).WithMany(user => user.Sites);

        builder.OwnsOne(
            s => s.Name,
            n =>
            {
                n.Property(p => p.Value).HasColumnName("Name").HasMaxLength(100).IsRequired();
            }
        );

        builder.OwnsOne(
            s => s.Address,
            a =>
            {
                a.Property(p => p.Value).HasColumnName("Address").HasMaxLength(200).IsRequired();
            }
        );
    }
}
