using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Interfaces;
using NSubstitute;

namespace Application.Tests.Invoices;

public sealed class InvoiceHandlerTests
{
    [Fact]
    public async Task Update_delegates_the_validated_invoice_details_to_the_invoice_service()
    {
        var invoices = Substitute.For<IInvoiceService>();
        var command = new UpdateInvoiceCommand { InvoiceId = Guid.NewGuid() };

        await new UpdateInvoiceHandler(invoices).Handle(command, CancellationToken.None);

        await invoices.Received(1).UpdateAsync(command, CancellationToken.None);
    }

    [Fact]
    public async Task SiteInvoices_uses_the_current_user_when_loading_allocated_invoices()
    {
        var invoices = Substitute.For<IInvoiceService>();
        var user = Substitute.For<IUser>();
        var siteId = Guid.NewGuid();
        user.Id.Returns("worker-1");
        invoices.GetSiteInvoicesAsync(siteId, "worker-1", CancellationToken.None)
            .Returns(Array.Empty<SiteInvoiceDto>());

        var result = await new SiteInvoicesQueryHandler(invoices, user)
            .Handle(new SiteInvoicesQuery(siteId), CancellationToken.None);

        Assert.Empty(result);
        await invoices.Received(1).GetSiteInvoicesAsync(siteId, "worker-1", CancellationToken.None);
    }

    [Fact]
    public async Task SiteInvoices_rejects_anonymous_users()
    {
        var invoices = Substitute.For<IInvoiceService>();
        var user = Substitute.For<IUser>();
        user.Id.Returns((string?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new SiteInvoicesQueryHandler(invoices, user)
                .Handle(new SiteInvoicesQuery(Guid.NewGuid()), CancellationToken.None));

        await invoices.DidNotReceiveWithAnyArgs().GetSiteInvoicesAsync(default, default!, default);
    }
}
