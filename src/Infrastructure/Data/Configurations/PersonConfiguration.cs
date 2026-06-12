using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");

        builder.Property(person => person.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(person => person.FirstName)
            .HasMaxLength(100);

        builder.Property(person => person.MiddleName)
            .HasMaxLength(100);

        builder.Property(person => person.LastName)
            .HasMaxLength(100);

        builder.Property(person => person.CompanyName)
            .HasMaxLength(250);

        builder.Property(person => person.Egn)
            .HasMaxLength(10);

        builder.Property(person => person.Eik)
            .HasMaxLength(13);

        builder.Property(person => person.VatNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(person => person.SearchName)
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(person => person.SearchTaxIdentifier)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(person => person.SearchName);
        builder.HasIndex(person => person.SearchTaxIdentifier);
        builder.HasIndex(person => person.Egn);
        builder.HasIndex(person => person.Eik);

        builder.HasMany(person => person.Addresses)
            .WithOne(address => address.Person)
            .HasForeignKey(address => address.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(person => person.Contacts)
            .WithOne(contact => contact.Person)
            .HasForeignKey(contact => contact.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(person => person.BankAccounts)
            .WithOne(bankAccount => bankAccount.Person)
            .HasForeignKey(bankAccount => bankAccount.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
