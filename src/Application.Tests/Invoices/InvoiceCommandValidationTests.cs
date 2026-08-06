using Application.Invoices.Commands;

namespace Application.Tests.Invoices;

public sealed class InvoiceCommandValidationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task CreateInvoice_allows_VAT_rates_at_the_supported_boundaries(decimal vatRate)
    {
        var result = await new CreateInvoiceValidator().ValidateAsync(ValidCommand(vatRate));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public async Task CreateInvoice_rejects_VAT_rates_outside_the_supported_range(decimal vatRate)
    {
        var result = await new CreateInvoiceValidator().ValidateAsync(ValidCommand(vatRate));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateInvoiceCommand.VatRate));
    }

    [Fact]
    public async Task CreateInvoice_rejects_duplicate_sites_and_allocations_over_the_VAT_inclusive_total()
    {
        var siteId = Guid.NewGuid();
        var command = ValidCommand(20m) with
        {
            SiteAllocations =
            [
                new InvoiceSiteAllocationInput { SiteId = siteId, Amount = 60m, Direction = "In" },
                new InvoiceSiteAllocationInput { SiteId = siteId, Amount = 61m, Direction = "Out" }
            ]
        };

        var result = await new CreateInvoiceValidator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.ErrorMessage == "A site can only be allocated once per invoice.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "The allocated total cannot exceed the invoice total including VAT.");
    }

    [Fact]
    public async Task UpdateInvoice_requires_an_invoice_identifier()
    {
        var command = new UpdateInvoiceCommand
        {
            SupplierId = Guid.NewGuid(), InvoiceNumber = "INV-42", Date = "2026-01-02T00:00:00+00:00",
            PaymentTerm = "2026-02-02T00:00:00+00:00", TotalValue = 100m, VatRate = 20m,
            PaymentMethod = "Transfer"
        };

        var result = await new UpdateInvoiceValidator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateInvoiceCommand.InvoiceId));
    }

    private static CreateInvoiceCommand ValidCommand(decimal vatRate) => new()
    {
        SupplierId = Guid.NewGuid(),
        InvoiceNumber = "INV-42",
        Date = "2026-01-02T00:00:00+00:00",
        PaymentTerm = "2026-02-02T00:00:00+00:00",
        TotalValue = 100m,
        VatRate = vatRate,
        PaymentMethod = "Transfer"
    };
}
