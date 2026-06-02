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
                        "DocumentType": "Invoice",
                        "SupplierName": "Acme Supplies Ltd",
                        "SupplierEik": "123456789",
                        "SupplierVatNumber": "BG123456789",
                        "BuyerName": "SiteWatch OOD",
                        "InvoiceNumber": "INV-2026-0001",
                        "InvoiceDate": "2026-06-01T00:00:00+03:00",
                        "Currency": "BGN",
                        "NetTotal": 100.00,
                        "NetTotalConfidence": 0.96,
                        "NetTotalBgn": 195.58,
                        "NetTotalBgnConfidence": 0.96,
                        "NetTotalEur": 100.00,
                        "NetTotalEurConfidence": 0.96,
                        "VatTotal": 20.00,
                        "VatTotalConfidence": 0.96,
                        "VatTotalBgn": 39.12,
                        "VatTotalBgnConfidence": 0.96,
                        "VatTotalEur": 20.00,
                        "VatTotalEurConfidence": 0.96,
                        "GrossTotal": 120.00,
                        "GrossTotalConfidence": 0.96,
                        "GrossTotalBgn": 234.70,
                        "GrossTotalBgnConfidence": 0.96,
                        "GrossTotalEur": 120.00,
                        "GrossTotalEurConfidence": 0.96,
                        "OverallConfidence": 0.96,
                        "Items": [
                          {
                            "ProductCode": "SKU-001",
                            "ProductName": "Monitoring subscription",
                            "Quantity": 2,
                            "Unit": "pcs",
                            "UnitPrice": 50.00,
                            "UnitPriceBgn": 97.79,
                            "UnitPriceBgnConfidence": 0.96,
                            "UnitPriceEur": 50.00,
                            "UnitPriceEurConfidence": 0.96,
                            "Discount": 0.00,
                            "DiscountBgn": 0.00,
                            "DiscountBgnConfidence": 0.96,
                            "DiscountEur": 0.00,
                            "DiscountEurConfidence": 0.96,
                            "VatRate": 20.00,
                            "LineTotal": 100.00,
                            "LineTotalBgn": 195.58,
                            "LineTotalBgnConfidence": 0.96,
                            "LineTotalEur": 100.00,
                            "LineTotalEurConfidence": 0.96,
                            "Confidence": 0.96
                          }
                        ],
                        "Issues": []
                      }
                      """;

        var rawOcrText = """
                         Supplier: Acme Supplies Ltd
                         Buyer: SiteWatch OOD
                         Invoice No: INV-2026-0001
                         Invoice Date: 2026-06-01
                         Total: 120.00 BGN
                         """;

        var result = new InvoiceExtractionResult
        {
            DocumentType = InvoiceExtractionDocumentType.Invoice,
            SupplierName = "Acme Supplies Ltd",
            SupplierEik = "123456789",
            SupplierVatNumber = "BG123456789",
            BuyerName = "SiteWatch OOD",
            InvoiceNumber = "INV-2026-0001",
            InvoiceDate = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.FromHours(3)),
            Currency = "BGN",
            DocumentTypeConfidence = 0.96m,
            SupplierNameConfidence = 0.96m,
            SupplierEikConfidence = 0.96m,
            SupplierVatNumberConfidence = 0.96m,
            BuyerNameConfidence = 0.96m,
            InvoiceNumberConfidence = 0.96m,
            InvoiceDateConfidence = 0.96m,
            CurrencyConfidence = 0.96m,
            NetTotal = 100.00m,
            NetTotalConfidence = 0.96m,
            NetTotalBgn = 195.58m,
            NetTotalBgnConfidence = 0.96m,
            NetTotalEur = 100.00m,
            NetTotalEurConfidence = 0.96m,
            VatTotal = 20.00m,
            VatTotalConfidence = 0.96m,
            VatTotalBgn = 39.12m,
            VatTotalBgnConfidence = 0.96m,
            VatTotalEur = 20.00m,
            VatTotalEurConfidence = 0.96m,
            GrossTotal = 120.00m,
            GrossTotalConfidence = 0.96m,
            GrossTotalBgn = 234.70m,
            GrossTotalBgnConfidence = 0.96m,
            GrossTotalEur = 120.00m,
            GrossTotalEurConfidence = 0.96m,
            OverallConfidence = 0.96m,
            Items = ImmutableArray.Create(
                new InvoiceExtractionLineResult
                {
                    ProductCode = "SKU-001",
                    ProductName = "Monitoring subscription",
                    Quantity = 2m,
                    Unit = "pcs",
                    UnitPrice = 50.00m,
                    UnitPriceBgn = 97.79m,
                    UnitPriceBgnConfidence = 0.96m,
                    UnitPriceEur = 50.00m,
                    UnitPriceEurConfidence = 0.96m,
                    Discount = 0.00m,
                    DiscountBgn = 0.00m,
                    DiscountBgnConfidence = 0.96m,
                    DiscountEur = 0.00m,
                    DiscountEurConfidence = 0.96m,
                    VatRate = 20.00m,
                    LineTotal = 100.00m,
                    LineTotalBgn = 195.58m,
                    LineTotalBgnConfidence = 0.96m,
                    LineTotalEur = 100.00m,
                    LineTotalEurConfidence = 0.96m,
                    Confidence = 0.96m
                }),
            Issues = ImmutableArray<InvoiceExtractionIssueResult>.Empty,
            RawJson = rawJson,
            RawOcrText = rawOcrText
        };

        return Task.FromResult<InvoiceExtractionResult?>(result);
    }
}
