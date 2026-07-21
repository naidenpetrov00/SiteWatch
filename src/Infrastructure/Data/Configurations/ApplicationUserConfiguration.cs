using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.NumberId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEXT VALUE FOR [dbo].[ApplicationUserNumberIds]")
            .IsRequired();

        builder.HasIndex(user => user.NumberId)
            .IsUnique();
    }
}
