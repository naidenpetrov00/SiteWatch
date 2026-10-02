using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Offers.Finalization;

/// <summary>Identifies an Offer product in finalization readiness details.</summary>
public sealed record OfferReadinessProductDto(
    Guid OfferProductLineId,
    int ProductNumberId,
    string Title);

/// <summary>Describes whether the current persisted Offer can be finalized.</summary>
public sealed record OfferFinalizationReadinessDto(
    Guid OfferId,
    string Status,
    string CurrencyCode,
    int SelectedActivityCount,
    int RequiredProductCount,
    int RequiredProductsWithSelectedPrices,
    IReadOnlyList<OfferReadinessProductDto> RequiredProductsMissingSelectedPrices,
    IReadOnlyList<OfferReadinessProductDto> RequiredProductsWithInvalidTotals,
    IReadOnlyList<OfferReadinessProductDto> OptionalProductsMissingSelectedPrices,
    IReadOnlyList<OfferReadinessProductDto> OptionalProductsWithInvalidTotals,
    decimal? SelectedRequiredTotal,
    decimal? SelectedOptionalTotal,
    bool RequiredPricingComplete,
    bool OptionalPricingComplete,
    IReadOnlyList<OfferReadinessProductDto> SelectedPricesWithNewerObservations,
    bool CanFinalize,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<string> Warnings);

public static class OfferFinalizationReadiness
{
    public static OfferFinalizationReadinessDto Evaluate(
        Offer offer,
        IReadOnlySet<Guid> linesWithNewerObservations)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(linesWithNewerObservations);

        var missingRequired = new List<OfferReadinessProductDto>();
        var invalidRequired = new List<OfferReadinessProductDto>();
        var missingOptional = new List<OfferReadinessProductDto>();
        var invalidOptional = new List<OfferReadinessProductDto>();
        var newer = new List<OfferReadinessProductDto>();
        decimal? requiredSubtotal = 0m;
        decimal? optionalSubtotal = null;
        var requiredOverflow = false;
        var optionalOverflow = false;

        var lines = offer.ProductLines
            .OrderBy(line => line.ProductNumberId)
            .ThenBy(line => line.Id)
            .ToList();
        foreach (var line in lines)
        {
            var product = new OfferReadinessProductDto(
                line.Id,
                line.ProductNumberId,
                line.Title);
            var selection = line.PriceSelection;
            if (selection is null)
            {
                if (line.RequiredQuantity > 0m) missingRequired.Add(product);
                if (line.OptionalQuantity > 0m) missingOptional.Add(product);
                continue;
            }

            if (linesWithNewerObservations.Contains(line.Id))
            {
                newer.Add(product);
            }

            if (line.RequiredQuantity > 0m)
            {
                if (TryTotal(line, line.RequiredQuantity, out var total))
                {
                    requiredSubtotal = Add(requiredSubtotal, total, ref requiredOverflow);
                }
                else
                {
                    invalidRequired.Add(product);
                }
            }

            if (line.OptionalQuantity > 0m)
            {
                if (TryTotal(line, line.OptionalQuantity, out var total))
                {
                    optionalSubtotal = Add(optionalSubtotal, total, ref optionalOverflow);
                }
                else
                {
                    invalidOptional.Add(product);
                }
            }
        }

        var reasons = new List<string>();
        if (offer.Status != OfferStatus.Draft)
        {
            reasons.Add("Only draft offers can be finalized.");
        }
        if (offer.Activities.Count == 0)
        {
            reasons.Add("Select at least one activity before finalizing.");
        }
        foreach (var product in missingRequired)
        {
            reasons.Add($"Required product #{product.ProductNumberId} · {product.Title} needs a selected price.");
        }
        foreach (var product in invalidRequired)
        {
            reasons.Add($"Required product #{product.ProductNumberId} · {product.Title} has a selected price that cannot produce a valid EUR total.");
        }
        if (requiredOverflow)
        {
            reasons.Add("The selected required EUR total exceeds the supported range.");
        }

        var warnings = new List<string>();
        foreach (var product in missingOptional)
        {
            warnings.Add($"Optional product #{product.ProductNumberId} · {product.Title} has no selected price.");
        }
        foreach (var product in invalidOptional)
        {
            warnings.Add($"Optional product #{product.ProductNumberId} · {product.Title} has a selected price that cannot produce a valid EUR total.");
        }
        if (optionalOverflow)
        {
            warnings.Add("The selected optional EUR total exceeds the supported range.");
        }
        foreach (var product in newer)
        {
            warnings.Add($"Selected price for product #{product.ProductNumberId} · {product.Title} has a newer retailer observation.");
        }

        return new OfferFinalizationReadinessDto(
            offer.Id,
            offer.Status.ToString(),
            RetailerPriceObservation.EuroCurrencyCode,
            offer.Activities.Count,
            lines.Count(line => line.RequiredQuantity > 0m),
            lines.Count(line => line.RequiredQuantity > 0m && line.PriceSelection is not null),
            missingRequired,
            invalidRequired,
            missingOptional,
            invalidOptional,
            requiredSubtotal,
            optionalSubtotal,
            missingRequired.Count == 0 && invalidRequired.Count == 0 && !requiredOverflow,
            missingOptional.Count == 0 && invalidOptional.Count == 0 && !optionalOverflow,
            newer,
            reasons.Count == 0,
            reasons,
            warnings);
    }

    private static bool TryTotal(
        OfferProductLine line,
        decimal quantity,
        out decimal total)
    {
        var selection = line.PriceSelection!;
        total = 0m;
        if (selection.Amount <= 0m
            || selection.CurrencyCode != RetailerPriceObservation.EuroCurrencyCode)
        {
            return false;
        }

        try
        {
            total = line.CalculatePriceTotal(quantity, selection.Amount, selection.Basis);
            return total > 0m;
        }
        catch (Exception exception) when (
            exception is InvalidOperationException
                or ArgumentOutOfRangeException
                or OverflowException)
        {
            return false;
        }
    }

    private static decimal? Add(
        decimal? subtotal,
        decimal amount,
        ref bool overflow)
    {
        if (overflow) return null;
        try
        {
            return (subtotal ?? 0m) + amount;
        }
        catch (OverflowException)
        {
            overflow = true;
            return null;
        }
    }
}
