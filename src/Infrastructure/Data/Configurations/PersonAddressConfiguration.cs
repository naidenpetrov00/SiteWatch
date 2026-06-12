using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PersonAddressConfiguration : IEntityTypeConfiguration<PersonAddress>
{
    public void Configure(EntityTypeBuilder<PersonAddress> builder)
    {
        builder.ToTable("PersonAddresses");

        builder.Property(address => address.AddressLine)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(address => address.AdditionalLine)
            .HasMaxLength(200);

        builder.Property(address => address.City)
            .HasMaxLength(100);

        builder.Property(address => address.PostalCode)
            .HasMaxLength(20);

        builder.Property(address => address.Country)
            .HasMaxLength(100);

        builder.Property(address => address.Details)
            .HasMaxLength(500);
    }
}
