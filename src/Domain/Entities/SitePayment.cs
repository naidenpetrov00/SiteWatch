using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class SitePayment : BaseAuditableEntity
{
    private SitePayment()
    {
    }

    public Guid InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;
    public Guid SiteId { get; private set; }
    public Site Site { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public SitePaymentDirection Direction { get; private set; }

    public static SitePayment Create(
        Invoice invoice,
        Site site,
        decimal amount,
        SitePaymentDirection direction)
    {
        var normalizedInvoice = Guard.Against.Null(invoice);
        var normalizedSite = Guard.Against.Null(site);

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentException("Amount must have at most two decimal places.", nameof(amount));
        }

        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction), "Unsupported payment direction.");
        }

        return new SitePayment
        {
            Id = Guid.NewGuid(),
            InvoiceId = normalizedInvoice.Id,
            Invoice = normalizedInvoice,
            SiteId = normalizedSite.Id,
            Site = normalizedSite,
            Amount = amount,
            Direction = direction
        };
    }
}
