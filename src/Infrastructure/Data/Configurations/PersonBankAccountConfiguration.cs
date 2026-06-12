using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PersonBankAccountConfiguration : IEntityTypeConfiguration<PersonBankAccount>
{
    public void Configure(EntityTypeBuilder<PersonBankAccount> builder)
    {
        builder.ToTable("PersonBankAccounts");

        builder.Property(bankAccount => bankAccount.IBAN)
            .HasMaxLength(34)
            .IsRequired();

        builder.Property(bankAccount => bankAccount.BIC)
            .HasMaxLength(11);

        builder.Property(bankAccount => bankAccount.BankName)
            .HasMaxLength(200);

        builder.Property(bankAccount => bankAccount.Details)
            .HasMaxLength(500);
    }
}
