using Domain.SeedWork.Enums;
using FluentValidation;

namespace Application.Invoices.Commands;

public sealed record InvoiceSiteAllocationInput
{
    public Guid SiteId { get; init; }
    public decimal Amount { get; init; }
    public string Direction { get; init; } = nameof(SitePaymentDirection.Out);
}

public sealed class InvoiceSiteAllocationInputValidator : AbstractValidator<InvoiceSiteAllocationInput>
{
    private const decimal MaximumAmount = 9999999999999999.99m;

    public InvoiceSiteAllocationInputValidator()
    {
        RuleFor(allocation => allocation.SiteId).NotEmpty();
        RuleFor(allocation => allocation.Amount)
            .GreaterThan(0m)
            .LessThanOrEqualTo(MaximumAmount)
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("Amount must have at most two decimal places.");
        RuleFor(allocation => allocation.Direction)
            .NotEmpty()
            .Must(BeSupportedDirection)
            .WithMessage("Direction must be In or Out.");
    }

    private static bool BeSupportedDirection(string direction) =>
        Enum.TryParse<SitePaymentDirection>(direction, true, out var parsedDirection)
        && Enum.IsDefined(parsedDirection);
}

internal static class InvoiceSiteAllocationValidation
{
    internal static bool HaveUniqueSites(IEnumerable<InvoiceSiteAllocationInput>? allocations)
    {
        if (allocations is null)
        {
            return false;
        }

        var siteIds = allocations.Select(allocation => allocation.SiteId).ToList();
        return siteIds.Distinct().Count() == siteIds.Count;
    }

    internal static bool FitWithinTotal(
        IEnumerable<InvoiceSiteAllocationInput>? allocations,
        decimal totalValueIncludingVat) =>
        allocations is not null
        && allocations.Sum(allocation => allocation.Amount) <= totalValueIncludingVat;
}
