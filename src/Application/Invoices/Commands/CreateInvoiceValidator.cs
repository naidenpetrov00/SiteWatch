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
        RuleFor(x => x.Iban)
            .NotEmpty()
            .MaximumLength(34);
        RuleFor(x => x.PaymentTerm)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.TotalValue)
            .GreaterThan(0m);
        RuleFor(x => x.VatRate)
            .InclusiveBetween(0m, 100m);
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .MaximumLength(100);

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
