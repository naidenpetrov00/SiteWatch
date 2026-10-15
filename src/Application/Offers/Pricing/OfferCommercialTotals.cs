using Domain.Entities;

namespace Application.Offers.Pricing;

/// <summary>Represents the current EUR commercial totals for an Offer.</summary>
public sealed record OfferCommercialTotalsDto(
    bool ActivityPricingComplete,
    bool ProductPricingComplete,
    decimal? ActivitySubtotalBeforeDiscount,
    decimal ActivityDiscountPercentage,
    decimal? ActivityDiscountAmount,
    decimal? ActivityTotalAfterDiscount,
    decimal? ProductSubtotalBeforeDiscount,
    decimal ProductDiscountPercentage,
    decimal? ProductDiscountAmount,
    decimal? ProductTotalAfterDiscount,
    decimal? CombinedOfferTotal);

public sealed record OfferCommercialTotalsResult(
    OfferCommercialTotalsDto Totals,
    bool DiscountsValid,
    bool ActivityTotalFailed,
    bool RequiredProductTotalFailed,
    bool OptionalProductTotalFailed,
    bool CombinedTotalFailed);

public static class OfferCommercialTotals
{
    public static OfferCommercialTotalsResult Calculate(Offer offer)
    {
        ArgumentNullException.ThrowIfNull(offer);

        var activityComplete = true;
        var activityFailed = false;
        decimal? activitySubtotal = 0m;
        foreach (var section in offer.Activities.SelectMany(activity => activity.Sections))
        {
            if (!section.PricingMode.HasValue)
            {
                activityComplete = false;
                continue;
            }

            try
            {
                var total = section.CalculatePriceTotal();
                if (!total.HasValue || total.Value < 0m)
                {
                    activityComplete = false;
                    activityFailed = true;
                    continue;
                }

                activitySubtotal = Add(activitySubtotal, total.Value, ref activityFailed);
            }
            catch (Exception exception) when (
                exception is InvalidOperationException or OverflowException)
            {
                activityComplete = false;
                activityFailed = true;
                activitySubtotal = null;
            }
        }

        var productComplete = true;
        var requiredProductFailed = false;
        var optionalProductFailed = false;
        decimal? requiredSubtotal = 0m;
        decimal? optionalSubtotal = 0m;
        foreach (var line in offer.ProductLines)
        {
            var selection = line.PriceSelection;
            if (line.RequiredQuantity > 0m)
            {
                if (selection is null)
                {
                    productComplete = false;
                }
                else if (TryProductTotal(line, line.RequiredQuantity, out var total))
                {
                    requiredSubtotal = Add(
                        requiredSubtotal,
                        total,
                        ref requiredProductFailed);
                }
                else
                {
                    productComplete = false;
                    requiredProductFailed = true;
                    requiredSubtotal = null;
                }
            }

            if (line.OptionalQuantity > 0m)
            {
                if (selection is null)
                {
                    productComplete = false;
                }
                else if (TryProductTotal(line, line.OptionalQuantity, out var total))
                {
                    optionalSubtotal = Add(
                        optionalSubtotal,
                        total,
                        ref optionalProductFailed);
                }
                else
                {
                    productComplete = false;
                    optionalProductFailed = true;
                    optionalSubtotal = null;
                }
            }
        }

        decimal? productSubtotal = null;
        if (requiredSubtotal.HasValue && optionalSubtotal.HasValue)
        {
            try
            {
                productSubtotal = requiredSubtotal.Value + optionalSubtotal.Value;
            }
            catch (OverflowException)
            {
                optionalProductFailed = offer.ProductLines.Any(line => line.OptionalQuantity > 0m);
                requiredProductFailed = !optionalProductFailed;
                productComplete = false;
            }
        }

        var discountsValid = IsValidDiscount(offer.ActivityDiscountPercentage)
            && IsValidDiscount(offer.ProductDiscountPercentage);
        var activityDiscountAmount = discountsValid
            ? CalculateDiscount(activitySubtotal, offer.ActivityDiscountPercentage)
            : null;
        var productDiscountAmount = discountsValid
            ? CalculateDiscount(productSubtotal, offer.ProductDiscountPercentage)
            : null;
        var activityTotal = Subtract(activitySubtotal, activityDiscountAmount);
        var productTotal = Subtract(productSubtotal, productDiscountAmount);
        decimal? combinedTotal = null;
        var combinedTotalFailed = false;
        if (activityTotal.HasValue && productTotal.HasValue)
        {
            try
            {
                combinedTotal = activityTotal.Value + productTotal.Value;
            }
            catch (OverflowException)
            {
                combinedTotal = null;
                combinedTotalFailed = true;
            }
        }

        return new OfferCommercialTotalsResult(
            new OfferCommercialTotalsDto(
                activityComplete && !activityFailed,
                productComplete && !requiredProductFailed && !optionalProductFailed,
                activitySubtotal,
                offer.ActivityDiscountPercentage,
                activityDiscountAmount,
                activityTotal,
                productSubtotal,
                offer.ProductDiscountPercentage,
                productDiscountAmount,
                productTotal,
                combinedTotal),
            discountsValid,
            activityFailed,
            requiredProductFailed,
            optionalProductFailed,
            combinedTotalFailed);
    }

    private static bool TryProductTotal(
        OfferProductLine line,
        decimal quantity,
        out decimal total)
    {
        total = 0m;
        var selection = line.PriceSelection!;
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

    private static decimal? Add(decimal? subtotal, decimal amount, ref bool failed)
    {
        if (failed || !subtotal.HasValue)
        {
            return null;
        }

        try
        {
            return subtotal.Value + amount;
        }
        catch (OverflowException)
        {
            failed = true;
            return null;
        }
    }

    private static decimal? CalculateDiscount(decimal? subtotal, decimal percentage)
    {
        if (!subtotal.HasValue)
        {
            return null;
        }

        return decimal.Round(
            subtotal.Value * (percentage / 100m),
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal? Subtract(decimal? subtotal, decimal? discount) =>
        subtotal.HasValue && discount.HasValue
            ? subtotal.Value - discount.Value
            : null;

    private static bool IsValidDiscount(decimal value) =>
        value is >= 0m and <= Offer.MaximumDiscountPercentage
        && decimal.Round(value, 2) == value;
}
