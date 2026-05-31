using Application.SeedWork.Interfaces;
using Domain.Entities;
using Domain.SeedWork.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Invoices.Commands;

public class UploadInvoiceCommand : IRequest<Guid>
{
    public Guid SiteId { get; init; }
    public required Stream Stream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long ContentLength { get; init; }
}

public class UploadInvoiceCommandHandler(
    IApplicationDbContext dbContext,
    IInvoiceFileStorage invoiceFileStorage)
    : IRequestHandler<UploadInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(UploadInvoiceCommand request, CancellationToken cancellationToken)
    {
        var site = await dbContext.Sites.FirstAsync(site => site.Id == request.SiteId, cancellationToken);

        var fileId = await invoiceFileStorage.UploadAsync(
            request.Stream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var documentType = DetermineDocumentType(request.ContentType, request.FileName);
        var invoice = InvoiceDocument.Create(
            request.SiteId,
            fileId,
            request.FileName,
            request.ContentType,
            documentType);

        site.AddInvoice(invoice);
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

        return invoice.Id;
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
