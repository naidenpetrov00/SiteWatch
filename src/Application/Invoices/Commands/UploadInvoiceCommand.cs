using Application.SeedWork.Interfaces;
using Domain.Entities;
using Domain.SeedWork.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Invoices.Commands;

public sealed record UploadInvoiceResult(
    Guid InvoiceDocumentId,
    string Status);

public class UploadInvoiceCommand : IRequest<UploadInvoiceResult>
{
    public Guid SiteId { get; init; }
    public required Stream Stream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long ContentLength { get; init; }
}

public class UploadInvoiceCommandHandler(
    IApplicationDbContext dbContext,
    IInvoiceFileStorage invoiceFileStorage,
    IMediator mediator)
    : IRequestHandler<UploadInvoiceCommand, UploadInvoiceResult>
{
    public async Task<UploadInvoiceResult> Handle(UploadInvoiceCommand request, CancellationToken cancellationToken)
    {
        _ = request.ContentLength;

        await dbContext.Sites.FirstAsync(site => site.Id == request.SiteId, cancellationToken);

        var fileId = await invoiceFileStorage.UploadAsync(
            request.Stream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var documentType = DetermineDocumentType(request.ContentType, request.FileName);
        var invoice = InvoiceDocument.Create(
            request.SiteId,
            request.FileName,
            fileId.ToString(),
            documentType);

        dbContext.InvoiceDocuments.Add(invoice);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await invoiceFileStorage.DeleteAsync(fileId, cancellationToken);
            throw;
        }

        try
        {
            await mediator.Send(new ProcessInvoiceCommand
            {
                SiteId = request.SiteId,
                InvoiceId = invoice.Id
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // The processing service persists the failed state before rethrowing.
        }

        return new UploadInvoiceResult(invoice.Id, invoice.Status.ToString());
    }

    private static InvoiceDocumentType DetermineDocumentType(string contentType, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) || extension == ".pdf")
        {
            return InvoiceDocumentType.Pdf;
        }

        return InvoiceDocumentType.Image;
    }
}
