using System.Collections.Immutable;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.External;

namespace Infrastructure.InvoiceExtraction;

internal sealed class FakeInvoiceExtractor : IInvoiceExtractor
{
    public Task<InvoiceExtractionResult?> ExtractAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        _ = stream;
        _ = contentType;
        _ = cancellationToken;

        var rawJson = """
                      {
                        "DocumentType": "Pdf",
                        "SupplierName": "Acme Supplies Ltd",
                        "SupplierEik": "123456789",
                        "SupplierVatNumber": "BG123456789",
                        "BuyerName": "SiteWatch OOD",
                        "InvoiceNumber": "INV-2026-0001",
                        "InvoiceDate": "2026-06-01T00:00:00+03:00",
                        "Currency": "BGN",
                        "NetTotal": 100.00,
                        "VatTotal": 20.00,
                        "GrossTotal": 120.00,
                        "OverallConfidence": 0.96,
                        "Items": [
                          {
                            "ProductCode": "SKU-001",
                            "ProductName": "Monitoring subscription",
                            "Quantity": 2,
                            "Unit": "pcs",
                            "UnitPrice": 50.00,
                            "Discount": 0.00,
                            "VatRate": 20.00,
                            "LineTotal": 100.00,
                            "Confidence": 0.96
                          }
                        ],
                        "Issues": []
                      }
                      """;

        var result = new InvoiceExtractionResult
        {
            DocumentType = "Pdf",
            SupplierName = "Acme Supplies Ltd",
            SupplierEik = "123456789",
            SupplierVatNumber = "BG123456789",
            BuyerName = "SiteWatch OOD",
            InvoiceNumber = "INV-2026-0001",
            InvoiceDate = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.FromHours(3)),
            Currency = "BGN",
            NetTotal = 100.00m,
            VatTotal = 20.00m,
            GrossTotal = 120.00m,
            OverallConfidence = 0.50m,
            Items = ImmutableArray.Create(
                new InvoiceExtractionLineResult
                {
                    ProductCode = "SKU-001",
                    ProductName = "Monitoring subscription",
                    Quantity = 2m,
                    Unit = "pcs",
                    UnitPrice = 50.00m,
                    Discount = 0.00m,
                    VatRate = 20.00m,
                    LineTotal = 100.00m,
                    Confidence = 0.50m
                }),
            Issues = ImmutableArray<InvoiceExtractionIssueResult>.Empty,
            RawJson = rawJson
        };

        return Task.FromResult<InvoiceExtractionResult?>(result);
    }
}
