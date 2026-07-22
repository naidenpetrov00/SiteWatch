using System.Globalization;
using FluentValidation;

namespace Application.Invoices.Commands;

public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Date)
            .NotEmpty()
            .Must(BeValidDateTimeOffset)
            .WithMessage("Date must be a valid date.");
        RuleFor(x => x.PaymentTerm)
            .NotEmpty()
            .Must(BeValidDateTimeOffset)
            .WithMessage("PaymentTerm must be a valid date.");
        RuleFor(x => x.TotalValue)
            .GreaterThan(0m);
        RuleFor(x => x.VatRate)
            .InclusiveBetween(0m, 100m);
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.SiteAllocations).NotNull();
        RuleForEach(x => x.SiteAllocations)
            .SetValidator(new InvoiceSiteAllocationInputValidator());
        RuleFor(x => x.SiteAllocations)
            .Must(InvoiceSiteAllocationValidation.HaveUniqueSites)
            .WithMessage("A site can only be allocated once per invoice.");
        RuleFor(x => x)
            .Must(HaveAllocationsWithinInvoiceTotal)
            .WithName(nameof(CreateInvoiceCommand.SiteAllocations))
            .WithMessage("The allocated total cannot exceed the invoice total including VAT.");

        When(x => !string.IsNullOrWhiteSpace(x.PaymentDate), () =>
        {
            RuleFor(x => x.PaymentDate)
                .Must(BeValidDateTimeOffset)
                .WithMessage("PaymentDate must be a valid date-time.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PaymentTime), () =>
        {
            RuleFor(x => x.PaymentTime)
                .Must(BeValidDateTimeOffset)
                .WithMessage("PaymentTime must be a valid date-time.");
        });
    }

    private static bool HaveAllocationsWithinInvoiceTotal(CreateInvoiceCommand command)
    {
        if (command.TotalValue <= 0m || command.VatRate < 0m || command.VatRate > 100m)
        {
            return true;
        }

        var vatAmount = Math.Round(
            command.TotalValue * command.VatRate / 100m,
            2,
            MidpointRounding.AwayFromZero);
        var totalValueIncludingVat = Math.Round(
            command.TotalValue + vatAmount,
            2,
            MidpointRounding.AwayFromZero);

        return InvoiceSiteAllocationValidation.FitWithinTotal(
            command.SiteAllocations,
            totalValueIncludingVat);
    }

    private static bool BeValidDateTimeOffset(string? value)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;
        return normalizedValue.Length > 0
            && DateTimeOffset.TryParse(
                normalizedValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out _
            );
    }
}
