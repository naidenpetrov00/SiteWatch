using Application.Invoices.Commands;
using Application.SeedWork.Interfaces;
using NSubstitute;

namespace Application.Tests.Invoices;

public sealed class InvoiceFileCommandHandlerTests
{
    [Fact]
    public async Task CreateFromFile_uploads_then_creates_an_incomplete_invoice_for_an_authorized_user()
    {
        var invoices = Substitute.For<IInvoiceService>();
        var blobs = Substitute.For<IInvoiceBlobService>();
        var user = Substitute.For<IUser>();
        user.Id.Returns("worker-1");
        var invoiceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var handler = new CreateInvoiceFromFileCommandHandler(invoices, blobs, user);
        await using var input = PdfFile();

        var result = await handler.Handle(new CreateInvoiceFromFileCommand(Guid.NewGuid(), input), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        await invoices.Received(1).EnsureUserCanAccessSiteAsync(Arg.Any<Guid>(), "worker-1", Arg.Any<CancellationToken>());
        await blobs.Received(1).UploadAsync(result, Arg.Any<UploadedInvoiceFile>(), Arg.Any<CancellationToken>());
        await invoices.Received(1).CreateIncompleteAsync(result, Arg.Any<Guid>(), "worker-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFromFile_deletes_the_uploaded_blob_when_invoice_creation_fails()
    {
        var invoices = Substitute.For<IInvoiceService>();
        invoices.CreateIncompleteAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("database failed")));
        var blobs = Substitute.For<IInvoiceBlobService>();
        var user = Substitute.For<IUser>();
        user.Id.Returns("worker-1");
        var handler = new CreateInvoiceFromFileCommandHandler(invoices, blobs, user);
        await using var input = PdfFile();

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new CreateInvoiceFromFileCommand(Guid.NewGuid(), input), CancellationToken.None));

        await blobs.Received(1).DeleteIfExistsAsync(Arg.Any<Guid>(), CancellationToken.None);
    }

    [Fact]
    public async Task CreateFromFile_rejects_anonymous_users_before_uploading()
    {
        var blobs = Substitute.For<IInvoiceBlobService>();
        var user = Substitute.For<IUser>();
        user.Id.Returns((string?)null);
        var handler = new CreateInvoiceFromFileCommandHandler(
            Substitute.For<IInvoiceService>(), blobs, user);
        await using var input = PdfFile();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new CreateInvoiceFromFileCommand(Guid.NewGuid(), input), CancellationToken.None));
        await blobs.DidNotReceiveWithAnyArgs().UploadAsync(default, default!, default);
    }

    [Fact]
    public async Task CreateFromFile_does_not_upload_when_the_user_cannot_access_the_site()
    {
        var invoices = Substitute.For<IInvoiceService>();
        invoices.EnsureUserCanAccessSiteAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new UnauthorizedAccessException()));
        var blobs = Substitute.For<IInvoiceBlobService>();
        var user = Substitute.For<IUser>();
        user.Id.Returns("worker-1");
        await using var input = PdfFile();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new CreateInvoiceFromFileCommandHandler(invoices, blobs, user)
            .Handle(new CreateInvoiceFromFileCommand(Guid.NewGuid(), input), CancellationToken.None));

        await blobs.DidNotReceiveWithAnyArgs().UploadAsync(default, default!, default);
    }

    [Fact]
    public async Task UploadFile_checks_that_the_invoice_exists_before_replacing_its_blob()
    {
        var invoices = Substitute.For<IInvoiceService>();
        var blobs = Substitute.For<IInvoiceBlobService>();
        var invoiceId = Guid.NewGuid();
        var handler = new UploadInvoiceFileCommandHandler(invoices, blobs);
        await using var input = PdfFile();

        await handler.Handle(new UploadInvoiceFileCommand(invoiceId, input), CancellationToken.None);

        await invoices.Received(1).EnsureInvoiceExistsAsync(invoiceId, CancellationToken.None);
        await blobs.Received(1).UploadAsync(invoiceId, Arg.Is<UploadedInvoiceFile>(file =>
            file.FileName == "invoice.pdf" && file.ContentType == "application/pdf"), CancellationToken.None);
    }

    [Fact]
    public async Task UploadFile_does_not_write_a_blob_when_validation_fails()
    {
        var blobs = Substitute.For<IInvoiceBlobService>();
        var handler = new UploadInvoiceFileCommandHandler(Substitute.For<IInvoiceService>(), blobs);
        await using var input = new UploadedInvoiceFile(
            new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]), "invoice.pdf", "image/png", 5);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => handler.Handle(
            new UploadInvoiceFileCommand(Guid.NewGuid(), input), CancellationToken.None));

        await blobs.DidNotReceiveWithAnyArgs().UploadAsync(default, default!, default);
    }

    private static UploadedInvoiceFile PdfFile() => new(
        new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]), "invoice.pdf", "application/pdf", 5);
}
