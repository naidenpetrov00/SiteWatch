using System.Text.Json;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Invoices.Services;

public sealed class InvoiceProcessingService(
    IApplicationDbContext dbContext,
    ILogger<InvoiceProcessingService> logger)
    : IInvoiceProcessingService
{
    public async Task ProcessAsync(
        Guid siteId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoiceDocument = await dbContext.InvoiceDocuments
            .FirstOrDefaultAsync(
                x => x.SiteId == siteId && x.Id == invoiceId,
                cancellationToken);

        if (invoiceDocument is null)
        {
            throw new NotFoundException(nameof(InvoiceDocument), invoiceId.ToString());
        }

        invoiceDocument.MarkProcessing();

        try
        {
            await dbContext.InvoiceLines
                .Where(x => x.InvoiceDocumentId == invoiceDocument.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await dbContext.InvoiceReviewIssues
                .Where(x => x.InvoiceDocumentId == invoiceDocument.Id)
                .ExecuteDeleteAsync(cancellationToken);

            invoiceDocument.CompleteProcessing(
                InvoiceExtractionStatus.Failed,
                DateTimeOffset.UtcNow,
                CreateFailurePayload(
                    "Invoice extraction workflow is not wired in yet. A new processing flow will replace it."));

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(
                ex,
                "Concurrency failure while processing invoice {InvoiceId} for site {SiteId}",
                invoiceId,
                siteId);

            await PersistFailureAsync(invoiceDocument, ex, cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected failure while processing invoice {InvoiceId} for site {SiteId}",
                invoiceId,
                siteId);

            await PersistFailureAsync(invoiceDocument, ex, cancellationToken);
            throw;
        }
    }

    private async Task PersistFailureAsync(
        InvoiceDocument invoiceDocument,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        invoiceDocument.CompleteProcessing(
            InvoiceExtractionStatus.Failed,
            DateTimeOffset.UtcNow,
            CreateFailurePayload(errorMessage));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistFailureAsync(
        InvoiceDocument invoiceDocument,
        Exception exception,
        CancellationToken cancellationToken)
    {
        invoiceDocument.CompleteProcessing(
            InvoiceExtractionStatus.Failed,
            DateTimeOffset.UtcNow,
            CreateFailurePayload(exception),
            null);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string CreateFailurePayload(string errorMessage)
        => JsonSerializer.Serialize(new
        {
            errorMessage,
            occurredAt = DateTimeOffset.UtcNow
        });

    private static string CreateFailurePayload(Exception exception)
        => JsonSerializer.Serialize(new
        {
            errorMessage = exception.Message,
            exceptionType = exception.GetType().FullName,
            occurredAt = DateTimeOffset.UtcNow
        });
}
