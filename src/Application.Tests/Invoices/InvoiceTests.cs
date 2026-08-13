using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Application.Tests.Invoices;

public sealed class InvoiceTests
{
    [Fact]
    public void Create_normalizes_invoice_fields_and_marks_invoice_complete()
    {
        var supplier = Person.CreateCompany("  Acme Ltd  ", CompanyLegalForm.ООД, "123456789", "BG123456789");

        var invoice = Invoice.Create(
            supplier.Id, supplier, "  INV-42  ", new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
            "  BG123  ", "  1 Main St  ", "  accounts@example.test ", "  +359  ", "  Ada  ",
            new DateTimeOffset(2026, 2, 2, 0, 0, 0, TimeSpan.Zero), 100m, 20m, 120m, "  Transfer  ");

        Assert.True(invoice.IsComplete);
        Assert.Equal(InvoiceStatus.Complete, invoice.Status);
        Assert.Equal("INV-42", invoice.InvoiceNumber);
        Assert.Equal("BG123", invoice.TaxIdentifier);
        Assert.Equal("1 Main St", invoice.Address);
        Assert.Equal("accounts@example.test", invoice.Email);
        Assert.Equal("+359", invoice.PhoneNumber);
        Assert.Equal("Ada", invoice.ContactPerson);
        Assert.Equal("Transfer", invoice.PaymentMethod);
    }

    [Fact]
    public void Create_rejects_a_supplier_whose_identifier_does_not_match()
    {
        var supplier = Person.CreateCompany("Acme Ltd", CompanyLegalForm.ООД, "123456789", "BG123456789");

        var exception = Assert.Throws<ArgumentException>(() => Invoice.Create(
            Guid.NewGuid(), supplier, "INV-42", DateTimeOffset.UtcNow, "BG123", "Address", null, null,
            "Ada", DateTimeOffset.UtcNow.AddDays(30), 100m, 20m, 120m, "Transfer"));

        Assert.Equal("supplierId", exception.ParamName);
    }

    [Fact]
    public void Incomplete_invoice_cannot_receive_site_payments_until_completed()
    {
        var invoice = Invoice.CreateIncomplete(Guid.NewGuid(), CreateSite());
        var payment = SitePayment.Create(invoice, CreateSite(), 10m, SitePaymentDirection.In);

        Assert.Throws<InvalidOperationException>(() => invoice.ReplaceSitePayments([payment]));
    }

    [Fact]
    public void ReplaceSitePayments_rejects_payments_for_another_invoice_or_total_above_invoice_value()
    {
        var invoice = CompleteInvoice();
        var otherInvoice = CompleteInvoice();
        var otherPayment = SitePayment.Create(otherInvoice, CreateSite(), 10m, SitePaymentDirection.In);
        var overTotalPayment = SitePayment.Create(invoice, CreateSite(), 120.01m, SitePaymentDirection.Out);

        Assert.Throws<ArgumentException>(() => invoice.ReplaceSitePayments([otherPayment]));
        Assert.Throws<ArgumentException>(() => invoice.ReplaceSitePayments([overTotalPayment]));
    }

    [Fact]
    public void ReplaceSitePayments_accepts_an_exact_total_but_rejects_duplicate_sites()
    {
        var invoice = CompleteInvoice();
        var site = CreateSite();
        var exactTotal = SitePayment.Create(invoice, site, 120m, SitePaymentDirection.In);
        var duplicateSite = SitePayment.Create(invoice, site, 1m, SitePaymentDirection.Out);

        invoice.ReplaceSitePayments([exactTotal]);

        Assert.Single(invoice.SitePayments);
        Assert.Throws<ArgumentException>(() => invoice.ReplaceSitePayments([exactTotal, duplicateSite]));
    }

    [Fact]
    public void CreateIncomplete_preserves_the_submitting_site_until_it_is_completed()
    {
        var site = CreateSite();
        var invoice = Invoice.CreateIncomplete(Guid.NewGuid(), site);

        Assert.False(invoice.IsComplete);
        Assert.Equal(InvoiceStatus.Incomplete, invoice.Status);
        Assert.Equal(site.Id, invoice.SubmittedFromSiteId);
        Assert.Same(site, invoice.SubmittedFromSite);
    }

    private static Invoice CompleteInvoice()
    {
        var supplier = Person.CreateCompany("Acme Ltd", CompanyLegalForm.ООД, "123456789", "BG123456789");
        return Invoice.Create(supplier.Id, supplier, "INV-42", DateTimeOffset.UtcNow, "BG123", "Address", null,
            null, "Ada", DateTimeOffset.UtcNow.AddDays(30), 100m, 20m, 120m, "Transfer");
    }

    private static Site CreateSite() => new(
        (SiteName)"Test site",
        (SiteAddress)"Test address",
        SiteMediaPolicy.FromPreset(MediaPolicyPreset.Custom));
}
