using Ardalis.GuardClauses;
using Domain.SeedWork;

namespace Domain.Entities;

public sealed class Invoice : BaseAuditableEntity, IHasNumberId
{
    private Invoice()
    {
    }

    public Guid SupplierId { get; private set; }
    public Person Supplier { get; private set; } = null!;
    public int NumberId { get; private set; }
    public string InvoiceNumber { get; private set; } = null!;
    public DateTimeOffset Date { get; private set; }
    public string Eik { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string ContactPerson { get; private set; } = null!;
    public string Iban { get; private set; } = null!;
    public string PaymentTerm { get; private set; } = null!;
    public decimal TotalValueExcludingVat { get; private set; }
    public decimal Vat { get; private set; }
    public decimal TotalValueIncludingVat { get; private set; }
    public DateTimeOffset? PaymentDate { get; private set; }
    public DateTimeOffset? PaymentTime { get; private set; }
    public string PaymentMethod { get; private set; } = null!;

    public static Invoice Create(
        Guid supplierId,
        Person supplier,
        string invoiceNumber,
        DateTimeOffset date,
        string eik,
        string address,
        string email,
        string phoneNumber,
        string contactPerson,
        string iban,
        string paymentTerm,
        decimal totalValueExcludingVat,
        decimal vat,
        decimal totalValueIncludingVat,
        string paymentMethod,
        DateTimeOffset? paymentDate = null,
        DateTimeOffset? paymentTime = null)
    {
        var normalizedSupplier = Guard.Against.Null(supplier, nameof(supplier));
        if (normalizedSupplier.Id != supplierId)
        {
            throw new ArgumentException("SupplierId must match the supplied supplier.", nameof(supplierId));
        }

        return new Invoice
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            Supplier = normalizedSupplier,
            InvoiceNumber = NormalizeRequiredText(invoiceNumber, nameof(invoiceNumber)),
            Date = date,
            Eik = NormalizeRequiredText(eik, nameof(eik)),
            Address = NormalizeRequiredText(address, nameof(address)),
            Email = NormalizeRequiredText(email, nameof(email)),
            PhoneNumber = NormalizeRequiredText(phoneNumber, nameof(phoneNumber)),
            ContactPerson = NormalizeRequiredText(contactPerson, nameof(contactPerson)),
            Iban = NormalizeRequiredText(iban, nameof(iban)),
            PaymentTerm = NormalizeRequiredText(paymentTerm, nameof(paymentTerm)),
            TotalValueExcludingVat = totalValueExcludingVat,
            Vat = vat,
            TotalValueIncludingVat = totalValueIncludingVat,
            PaymentMethod = NormalizeRequiredText(paymentMethod, nameof(paymentMethod)),
            PaymentDate = paymentDate,
            PaymentTime = paymentTime
        };
    }

    private static string NormalizeRequiredText(string value, string parameterName) =>
        Guard.Against.NullOrWhiteSpace(value, parameterName).Trim();
}