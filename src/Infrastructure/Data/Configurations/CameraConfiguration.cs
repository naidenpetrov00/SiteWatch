using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CameraConfiguration : IEntityTypeConfiguration<Camera>
{
    private const int CredentialMaxLength = 50;

    public void Configure(EntityTypeBuilder<Camera> builder)
    {
        builder.HasOne(site => site.Site).WithMany(site => site.Cameras);

        builder.OwnsOne(
            c => c.CameraName,
            n => { n.Property(p => p.Value).HasColumnName("Name").HasMaxLength(100).IsRequired(); }
        );
        builder.OwnsOne(
            c => c.CameraBrand,
            n =>
            {
                n.Property(p => p.Brand).HasColumnName("Brand").HasConversion<int>().IsRequired();
                n.Property(p => p.Model).HasColumnName("Model").HasMaxLength(100).IsRequired();
            }
        );

        builder.Property(c => c.IpAddress).HasMaxLength(39);
        builder.Property(c => c.RtspPort).HasMaxLength(5);
        builder.Property(c => c.Username).HasMaxLength(CredentialMaxLength);
        builder.Property(c => c.Password).HasMaxLength(CredentialMaxLength);
    }
}
