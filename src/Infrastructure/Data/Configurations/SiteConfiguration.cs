using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.HasMany(site => site.Users).WithMany(user => user.Sites);

        var mediaPolicyConverter = new ValueConverter<SiteMediaPolicy, string>(
            mediaPolicy => mediaPolicy.ToStorageValue(),
            value => SiteMediaPolicy.FromStorageValue(value));

        var mediaPolicyComparer = new ValueComparer<SiteMediaPolicy>(
            (left, right) => left == right,
            mediaPolicy => mediaPolicy.GetHashCode(),
            mediaPolicy => SiteMediaPolicy.FromStorageValue(mediaPolicy.ToStorageValue()));

        builder.Property(site => site.MediaPolicy)
            .HasColumnName("MediaPolicy")
            .HasColumnType("nvarchar(max)")
            .HasConversion(mediaPolicyConverter)
            .Metadata.SetValueComparer(mediaPolicyComparer);

        builder.OwnsOne(
            s => s.Name,
            n => { n.Property(p => p.Value).HasColumnName("Name").HasMaxLength(100).IsRequired(); }
        );

        builder.OwnsOne(
            s => s.Address,
            a => { a.Property(p => p.Value).HasColumnName("Address").HasMaxLength(200).IsRequired(); }
        );
    }
}
