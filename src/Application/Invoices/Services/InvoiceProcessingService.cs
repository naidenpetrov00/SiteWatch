using System.Text.Json;
using System.Text.Json.Serialization;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.External;
using Application.SeedWork.Exceptions;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Invoices.Services;

public sealed class InvoiceProcessingService(
    IApplicationDbContext dbContext,
    IInvoiceFileStorage invoiceFileStorage,
    IInvoiceExtractor invoiceExtractor,
    IInvoiceValidationService invoiceValidationService,
    ILogger<InvoiceProcessingService> logger)
    : IInvoiceProcessingService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

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
            var fileId = Guid.Parse(invoiceDocument.StoredFilePath);
            var fileResponse = await invoiceFileStorage.DownloadAsync(fileId, cancellationToken);

            await using var stream = fileResponse.Stream;
            var extractedResult = await invoiceExtractor.ExtractAsync(
                stream,
                fileResponse.ContentType,
                cancellationToken);

            if (extractedResult is null)
            {
                await PersistFailureAsync(
                    invoiceDocument,
                    "Invoice extraction returned no result.",
                    cancellationToken);
                return;
            }

            var validationResult = await invoiceValidationService.ValidateAsync(
                extractedResult,
                cancellationToken);

            var mappedLines = MapLines(invoiceDocument.Id, extractedResult);
            var mappedIssues = MapIssues(invoiceDocument.Id, extractedResult, validationResult);
            var finalStatus = mappedIssues.Any()
                ? InvoiceExtractionStatus.NeedsReview
                : InvoiceExtractionStatus.Extracted;
            var rawJson = SerializeExtractionResult(extractedResult);

            await dbContext.InvoiceLines
                .Where(x => x.InvoiceDocumentId == invoiceDocument.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await dbContext.InvoiceReviewIssues
                .Where(x => x.InvoiceDocumentId == invoiceDocument.Id)
                .ExecuteDeleteAsync(cancellationToken);

            invoiceDocument.ApplyExtractionFields(
                extractedResult.SupplierName,
                extractedResult.SupplierEik,
                extractedResult.SupplierVatNumber,
                extractedResult.BuyerName,
                extractedResult.InvoiceNumber,
                extractedResult.InvoiceDate,
                extractedResult.Currency,
                extractedResult.NetTotal,
                extractedResult.VatTotal,
                extractedResult.GrossTotal,
                extractedResult.OverallConfidence);

            dbContext.InvoiceLines.AddRange(mappedLines);
            dbContext.InvoiceReviewIssues.AddRange(mappedIssues);
            invoiceDocument.CompleteProcessing(finalStatus, DateTimeOffset.UtcNow, rawJson);

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
            CreateFailurePayload(exception));

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
            rawResponse = exception is OpenRouterInvoiceExtractionException openRouterException
                ? openRouterException.RawResponse
                : null,
            statusCode = exception is OpenRouterInvoiceExtractionException statusException
                ? statusException.StatusCode
                : null,
            occurredAt = DateTimeOffset.UtcNow
        });

    private static IReadOnlyCollection<InvoiceLine> MapLines(
        Guid invoiceDocumentId,
        InvoiceExtractionResult extractedResult)
        => extractedResult.Items
            .Select(x => InvoiceLine.Create(
                invoiceDocumentId,
                x.ProductCode,
                x.ProductName,
                x.Quantity,
                x.Unit,
                x.UnitPrice,
                x.Discount,
                x.VatRate,
                x.LineTotal,
                x.Confidence))
            .ToArray();

    private static IReadOnlyCollection<InvoiceReviewIssue> MapIssues(
        Guid invoiceDocumentId,
        InvoiceExtractionResult extractedResult,
        InvoiceValidationResult validationResult)
    {
        var extractionIssues = extractedResult.Issues
            .Select(issue => InvoiceReviewIssue.Create(
                invoiceDocumentId,
                issue.FieldPath,
                issue.ExtractedValue,
                issue.Reason,
                issue.Confidence));

        var validationIssues = validationResult.Issues.Select(issue => InvoiceReviewIssue.Create(
            invoiceDocumentId,
            issue.FieldPath,
            issue.ExtractedValue,
            issue.Reason,
            issue.Confidence));

        return extractionIssues.Concat(validationIssues).ToArray();
    }

    private static string SerializeExtractionResult(InvoiceExtractionResult extractedResult)
        => extractedResult.RawJson ?? JsonSerializer.Serialize(extractedResult, JsonSerializerOptions);
}
