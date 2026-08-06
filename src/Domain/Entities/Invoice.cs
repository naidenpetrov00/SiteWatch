using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Invoice : BaseAuditableEntity, IHasNumberId
{
    private readonly HashSet<SitePayment> _sitePayments = [];

    private Invoice()
    {
    }

    public Guid? SupplierId { get; private set; }
    public Person? Supplier { get; private set; }
    public Guid? SubmittedFromSiteId { get; private set; }
    public Site? SubmittedFromSite { get; private set; }
    public int NumberId { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public bool IsComplete => Status == InvoiceStatus.Complete;
    public string? InvoiceNumber { get; private set; }
    public DateTimeOffset? Date { get; private set; }
    public string? TaxIdentifier { get; private set; }
    public string? Address { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? ContactPerson { get; private set; }
    public DateTimeOffset? PaymentTerm { get; private set; }
    public decimal? TotalValueExcludingVat { get; private set; }
    public decimal? VatRate { get; private set; }
    public decimal? Vat { get; private set; }
    public decimal? TotalValueIncludingVat { get; private set; }
    public DateTimeOffset? PaymentDate { get; private set; }
    public DateTimeOffset? PaymentTime { get; private set; }
    public string? PaymentMethod { get; private set; }
    public IReadOnlyCollection<SitePayment> SitePayments => _sitePayments;

    public static Invoice Create(
        Guid supplierId,
        Person supplier,
        string invoiceNumber,
        DateTimeOffset date,
        string taxIdentifier,
        string address,
        string? email,
        string? phoneNumber,
        string contactPerson,
        DateTimeOffset paymentTerm,
        decimal totalValueExcludingVat,
        decimal vat,
        decimal totalValueIncludingVat,
        string paymentMethod,
        DateTimeOffset? paymentDate = null,
        DateTimeOffset? paymentTime = null,
        decimal? vatRate = null)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
        };

        invoice.SetStructuredData(
            supplierId,
            supplier,
            invoiceNumber,
            date,
            taxIdentifier,
            address,
            email,
            phoneNumber,
            contactPerson,
            paymentTerm,
            totalValueExcludingVat,
            vatRate,
            vat,
            totalValueIncludingVat,
            paymentMethod,
            paymentDate,
            paymentTime);

        return invoice;
    }

    public static Invoice CreateIncomplete(Guid id, Site submittedFromSite)
    {
        var normalizedSite = Guard.Against.Null(submittedFromSite);

        return new Invoice
        {
            Id = Guard.Against.Default(id),
            Created = DateTimeOffset.UtcNow,
            SubmittedFromSiteId = normalizedSite.Id,
            SubmittedFromSite = normalizedSite,
            Status = InvoiceStatus.Incomplete
        };
    }

    public void CompleteOrUpdate(
        Guid supplierId,
        Person supplier,
        string invoiceNumber,
        DateTimeOffset date,
        string taxIdentifier,
        string address,
        string? email,
        string? phoneNumber,
        string contactPerson,
        DateTimeOffset paymentTerm,
        decimal totalValueExcludingVat,
        decimal vatRate,
        decimal vat,
        decimal totalValueIncludingVat,
        string paymentMethod,
        DateTimeOffset? paymentDate = null,
        DateTimeOffset? paymentTime = null)
    {
        SetStructuredData(
            supplierId,
            supplier,
            invoiceNumber,
            date,
            taxIdentifier,
            address,
            email,
            phoneNumber,
            contactPerson,
            paymentTerm,
            totalValueExcludingVat,
            vatRate,
            vat,
            totalValueIncludingVat,
            paymentMethod,
            paymentDate,
            paymentTime);
    }

    public void ReplaceSitePayments(IEnumerable<SitePayment> sitePayments)
    {
        var normalizedSitePayments = Guard.Against.Null(sitePayments).ToList();

        if (normalizedSitePayments.Any(sitePayment => sitePayment.InvoiceId != Id))
        {
            throw new ArgumentException("Every site payment must belong to this invoice.", nameof(sitePayments));
        }

        if (normalizedSitePayments.Select(sitePayment => sitePayment.SiteId).Distinct().Count()
            != normalizedSitePayments.Count)
        {
            throw new ArgumentException("A site can only be allocated once per invoice.", nameof(sitePayments));
        }

        if (normalizedSitePayments.Count > 0 && !TotalValueIncludingVat.HasValue)
        {
            throw new InvalidOperationException(
                "Site payments cannot be assigned until the invoice is complete.");
        }

        if (TotalValueIncludingVat.HasValue
            && normalizedSitePayments.Sum(sitePayment => sitePayment.Amount)
                > TotalValueIncludingVat.Value)
        {
            throw new ArgumentException(
                "The allocated total cannot exceed the invoice total including VAT.",
                nameof(sitePayments));
        }

        _sitePayments.Clear();
        foreach (var sitePayment in normalizedSitePayments)
        {
            _sitePayments.Add(sitePayment);
        }
    }

    private void SetStructuredData(
        Guid supplierId,
        Person supplier,
        string invoiceNumber,
        DateTimeOffset date,
        string taxIdentifier,
        string address,
        string? email,
        string? phoneNumber,
        string contactPerson,
        DateTimeOffset paymentTerm,
        decimal totalValueExcludingVat,
        decimal? vatRate,
        decimal vat,
        decimal totalValueIncludingVat,
        string paymentMethod,
        DateTimeOffset? paymentDate,
        DateTimeOffset? paymentTime)
    {
        var normalizedSupplier = Guard.Against.Null(supplier, nameof(supplier));
        if (normalizedSupplier.Id != supplierId)
        {
            throw new ArgumentException("SupplierId must match the supplied supplier.", nameof(supplierId));
        }

        SupplierId = supplierId;
        Supplier = normalizedSupplier;
        InvoiceNumber = NormalizeRequiredText(invoiceNumber, nameof(invoiceNumber));
        Date = date;
        TaxIdentifier = NormalizeRequiredText(taxIdentifier, nameof(taxIdentifier));
        Address = NormalizeRequiredText(address, nameof(address));
        Email = NormalizeOptionalText(email);
        PhoneNumber = NormalizeOptionalText(phoneNumber);
        ContactPerson = NormalizeRequiredText(contactPerson, nameof(contactPerson));
        PaymentTerm = paymentTerm;
        TotalValueExcludingVat = totalValueExcludingVat;
        VatRate = vatRate;
        Vat = vat;
        TotalValueIncludingVat = totalValueIncludingVat;
        PaymentMethod = NormalizeRequiredText(paymentMethod, nameof(paymentMethod));
        PaymentDate = paymentDate;
        PaymentTime = paymentTime;
        Status = InvoiceStatus.Complete;
    }

    private static string NormalizeRequiredText(string value, string parameterName) =>
        Guard.Against.NullOrWhiteSpace(value, parameterName).Trim();

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
