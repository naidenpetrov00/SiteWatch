using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PersonContactConfiguration : IEntityTypeConfiguration<PersonContact>
{
    public void Configure(EntityTypeBuilder<PersonContact> builder)
    {
        builder.ToTable("PersonContacts");

        builder.Property(contact => contact.ContactType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(contact => contact.Value)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(contact => contact.Details)
            .HasMaxLength(500);
    }
}
