using System.Globalization;
using Domain.Entities;
using Domain.SeedWork.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Proposals;

internal sealed class ProposalPdfRenderer
{
    public byte[] Render(Proposal proposal, DateTimeOffset issuedAt)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        proposal.EnsureCanIssue();

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(style => style.FontSize(9));
                page.Header().Column(column =>
                {
                    column.Item().Text($"Proposal #{proposal.NumberId}")
                        .FontSize(22)
                        .SemiBold();
                    column.Item().Text($"Revision {proposal.RevisionNumber}")
                        .FontSize(11)
                        .FontColor(Colors.Grey.Darken1);
                });
                page.Content().PaddingVertical(18).Column(column =>
                {
                    column.Spacing(14);
                    ComposeIdentity(column, proposal, issuedAt);
                    ComposeActivities(column, proposal);
                    ComposeProducts(column, proposal);
                    ComposeTotals(column, proposal);
                    ComposeMetadata(column, proposal);
                });
                page.Footer().AlignCenter().Text(
                    $"Proposal #{proposal.NumberId} · Revision {proposal.RevisionNumber}")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }

    private static void ComposeIdentity(
        ColumnDescriptor column,
        Proposal proposal,
        DateTimeOffset issuedAt)
    {
        var validUntil = proposal.ValidUntil
            ?? throw new InvalidOperationException(
                "A Proposal validity date is required for PDF generation.");
        column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(box =>
        {
            box.Spacing(4);
            box.Item().Text($"Issue date: {issuedAt:yyyy-MM-dd}");
            box.Item().Text($"Valid until: {validUntil:yyyy-MM-dd}");
            box.Item().Text($"Source Offer: #{proposal.SourceOfferNumberId}");
            box.Item().Text($"Site: #{proposal.SiteNumberId} · {proposal.SiteName}");
            box.Item().Text($"Address: {proposal.SiteAddress}");
            box.Item().Text($"Recipient: {proposal.RecipientDisplayName}");
            box.Item().Text($"Recipient email: {proposal.RecipientEmail}");
        });
    }

    private static void ComposeActivities(ColumnDescriptor column, Proposal proposal)
    {
        column.Item().Text("Activities").FontSize(15).SemiBold();
        foreach (var activity in proposal.Activities.OrderBy(item => item.SortOrder))
        {
            column.Item().Text($"#{activity.ActivityNumberId} · {activity.Name}").SemiBold();
            if (!string.IsNullOrWhiteSpace(activity.Description))
            {
                column.Item().Text(activity.Description).FontColor(Colors.Grey.Darken1);
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Section");
                    header.Cell().Element(HeaderCellStyle).Text("Measurement");
                    header.Cell().Element(HeaderCellStyle).Text("Pricing");
                    header.Cell().Element(HeaderCellStyle).Text("Total");
                });
                foreach (var section in activity.Sections.OrderBy(item => item.SortOrder))
                {
                    BodyCell(table, section.Name ?? "Measurement");
                    BodyCell(table, $"{section.RequestedMeasurement:0.####} {section.MeasurementUnit.ToCode()}");
                    BodyCell(
                        table,
                        section.PricingMode == Domain.SeedWork.Enums.ActivityPricingMode.Free
                            ? "Free"
                            : $"{section.PricingMode.ToCode()} · {Money(section.PriceAmount ?? 0m)}");
                    BodyCell(table, Money(section.CalculatedTotal));
                }
            });
        }
    }

    private static void ComposeProducts(ColumnDescriptor column, Proposal proposal)
    {
        column.Item().Text("Products").FontSize(15).SemiBold();
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });
            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Product");
                header.Cell().Element(HeaderCellStyle).Text("Quantity");
                header.Cell().Element(HeaderCellStyle).Text("Selected price");
                header.Cell().Element(HeaderCellStyle).Text("Calculated total");
            });
            foreach (var product in proposal.ProductLines.OrderBy(item => item.SortOrder))
            {
                BodyCell(table, $"#{product.ProductNumberId} · {product.Title}");
                BodyCell(
                    table,
                    $"Required {product.RequiredQuantity:0.########}; Optional {product.OptionalQuantity:0.########}");
                if (product.IsOptionalPriceExcluded)
                {
                    BodyCell(table, "Optional — price not included");
                    BodyCell(table, "Excluded");
                    continue;
                }

                BodyCell(
                    table,
                    $"{Money(product.SelectedPriceAmount ?? 0m)} / {product.SelectedPriceBasis?.ToCode()}"
                    + (string.IsNullOrWhiteSpace(product.SelectedRetailerDisplayName)
                        ? string.Empty
                        : $" · {product.SelectedRetailerDisplayName}"));
                BodyCell(
                    table,
                    $"Required {Money(product.RequiredTotal ?? 0m)}; Optional {Money(product.OptionalTotal ?? 0m)}");
            }
        });
    }

    private static void ComposeTotals(ColumnDescriptor column, Proposal proposal)
    {
        column.Item().Text("Commercial Summary").FontSize(15).SemiBold();
        column.Item().AlignRight().Column(totals =>
        {
            totals.Spacing(3);
            totals.Item().Text($"Activities subtotal: {Money(proposal.ActivitySubtotalBeforeDiscount)}");
            totals.Item().Text(
                $"Activity discount ({proposal.ActivityDiscountPercentage:0.##}%): -{Money(proposal.ActivityDiscountAmount)}");
            totals.Item().Text($"Activities total: {Money(proposal.ActivityTotalAfterDiscount)}");
            totals.Item().Text($"Products subtotal: {Money(proposal.ProductSubtotalBeforeDiscount)}");
            totals.Item().Text(
                $"Product discount ({proposal.ProductDiscountPercentage:0.##}%): -{Money(proposal.ProductDiscountAmount)}");
            totals.Item().Text($"Products total: {Money(proposal.ProductTotalAfterDiscount)}");
            totals.Item().PaddingTop(4).Text($"Proposal total: {Money(proposal.Total)}")
                .FontSize(14)
                .SemiBold();
        });
        if (proposal.ExcludesUnpricedOptionalItems)
        {
            column.Item()
                .Background(Colors.Orange.Lighten4)
                .Padding(8)
                .Text("This Proposal total excludes unpriced optional items.")
                .SemiBold();
        }
    }

    private static void ComposeMetadata(ColumnDescriptor column, Proposal proposal)
    {
        column.Item().Text("Public Notes").FontSize(13).SemiBold();
        column.Item().Text(proposal.PublicNotes ?? "None");
        column.Item().Text("Payment Terms").FontSize(13).SemiBold();
        column.Item().Text(proposal.PaymentTerms ?? "Not specified");
    }

    private static IContainer HeaderCellStyle(IContainer container) =>
        container
            .Background(Colors.Grey.Lighten3)
            .DefaultTextStyle(style => style.SemiBold())
            .Padding(5);

    private static void BodyCell(TableDescriptor table, string text) =>
        table.Cell()
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5)
            .Text(text);

    private static string Money(decimal amount) =>
        $"{amount.ToString("N2", CultureInfo.InvariantCulture)} EUR";
}
