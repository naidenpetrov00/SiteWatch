using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PersonAddressConfiguration : IEntityTypeConfiguration<PersonAddress>
{
    public void Configure(EntityTypeBuilder<PersonAddress> builder)
    {
        builder.ToTable("PersonAddresses");

        builder.Property(address => address.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(address => address.Municipality).HasMaxLength(100);
        builder.Property(address => address.Settlement).HasMaxLength(100);
        builder.Property(address => address.District).HasMaxLength(100);
        builder.Property(address => address.Quarter).HasMaxLength(100);
        builder.Property(address => address.PostalCode).HasMaxLength(20);
        builder.Property(address => address.Street).HasMaxLength(200);
        builder.Property(address => address.Number).HasMaxLength(50);
        builder.Property(address => address.Building).HasMaxLength(50);
        builder.Property(address => address.Entrance).HasMaxLength(50);
        builder.Property(address => address.Floor).HasMaxLength(50);
        builder.Property(address => address.Apartment).HasMaxLength(50);

        builder.Property(address => address.AddressDetails)
            .HasMaxLength(500);
    }
}
