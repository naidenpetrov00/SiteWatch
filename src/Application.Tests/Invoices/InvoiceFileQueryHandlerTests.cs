using Application.Invoices.Queries;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Application.SeedWork.Security;
using NSubstitute;

namespace Application.Tests.Invoices;

public sealed class InvoiceFileQueryHandlerTests
{
    [Fact]
    public async Task FileAccess_checks_the_current_users_invoice_access_before_returning_file_metadata()
    {
        var invoices = Substitute.For<IInvoiceService>();
        var blobs = Substitute.For<IInvoiceBlobService>();
        var user = Substitute.For<IUser>();
        var siteId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        user.Id.Returns("worker-1");
        blobs.GetInfoAsync(invoiceId, Arg.Any<CancellationToken>())
            .Returns(new InvoiceFileInfoDto("invoice.pdf", "application/pdf"));

        var result = await new InvoiceFileAccessQueryHandler(invoices, blobs, user)
            .Handle(new InvoiceFileAccessQuery(siteId, invoiceId), CancellationToken.None);

        Assert.Equal("invoice.pdf", result.FileName);
        await invoices.Received(1).EnsureUserCanAccessInvoiceAsync(siteId, invoiceId, "worker-1", CancellationToken.None);
        await blobs.Received(1).GetInfoAsync(invoiceId, CancellationToken.None);
    }

    [Fact]
    public async Task FileAccess_rejects_anonymous_users_without_accessing_the_blob()
    {
        var blobs = Substitute.For<IInvoiceBlobService>();
        var user = Substitute.For<IUser>();
        user.Id.Returns((string?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new InvoiceFileAccessQueryHandler(Substitute.For<IInvoiceService>(), blobs, user)
                .Handle(new InvoiceFileAccessQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));

        await blobs.DidNotReceiveWithAnyArgs().GetInfoAsync(default, default);
    }

    [Fact]
    public async Task FileDownload_rejects_users_without_an_administrator_or_worker_role()
    {
        var identity = Substitute.For<IIdentityService>();
        identity.IsInRoleAsync("client-1", UserRoles.Administrator).Returns(false);
        identity.IsInRoleAsync("client-1", UserRoles.Worker).Returns(false);
        var blobs = Substitute.For<IInvoiceBlobService>();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            new InvoiceFileDownloadQueryHandler(identity, Substitute.For<IInvoiceService>(), blobs)
                .Handle(new InvoiceFileDownloadQuery(Guid.NewGuid(), Guid.NewGuid(), "client-1"), CancellationToken.None));

        await blobs.DidNotReceiveWithAnyArgs().DownloadAsync(default, default);
    }

    [Fact]
    public async Task FileDownload_authorizes_then_returns_the_invoice_stream()
    {
        var identity = Substitute.For<IIdentityService>();
        var invoices = Substitute.For<IInvoiceService>();
        var blobs = Substitute.For<IInvoiceBlobService>();
        var siteId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var response = new InvoiceFileResponse(new MemoryStream([1, 2, 3]), "invoice.pdf", "application/pdf", 3);
        identity.IsInRoleAsync("worker-1", UserRoles.Administrator).Returns(false);
        identity.IsInRoleAsync("worker-1", UserRoles.Worker).Returns(true);
        blobs.DownloadAsync(invoiceId, CancellationToken.None).Returns(response);

        var result = await new InvoiceFileDownloadQueryHandler(identity, invoices, blobs)
            .Handle(new InvoiceFileDownloadQuery(siteId, invoiceId, "worker-1"), CancellationToken.None);

        Assert.Same(response, result);
        await invoices.Received(1).EnsureUserCanAccessInvoiceAsync(siteId, invoiceId, "worker-1", CancellationToken.None);
    }
}
