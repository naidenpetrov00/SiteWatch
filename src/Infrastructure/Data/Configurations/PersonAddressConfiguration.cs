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

        builder.HasOne(address => address.Municipality).WithMany().HasForeignKey(address => address.MunicipalityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(address => address.Settlement).WithMany().HasForeignKey(address => address.SettlementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(address => address.District).WithMany().HasForeignKey(address => address.DistrictId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(address => address.Quarter).WithMany().HasForeignKey(address => address.QuarterId).OnDelete(DeleteBehavior.Restrict);
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
