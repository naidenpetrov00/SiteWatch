using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Products.Commands;

public abstract class ProductUpsertValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : ProductUpsertDto
{
    protected ProductUpsertValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(2000);
        RuleFor(request => request.Brand).MaximumLength(100);
        RuleFor(request => request.Model).MaximumLength(100);
        RuleFor(request => request.PackageUnit)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || ProductPackageUnitCodes.TryParse(value, out _))
            .WithMessage("PackageUnit must be a supported product package unit.");
        RuleFor(request => request.Category)
            .Must(value => ProductCategoryCodes.TryParse(value, out _))
            .WithMessage("Category must be a supported product category.");
        RuleFor(request => request.Status)
            .Must(IsSupportedStatus)
            .WithMessage("Status must be Active, Unavailable, or Discontinued.");
        RuleFor(request => request.PrimarySearchPhrase)
            .MaximumLength(ProductSearchConfiguration.MaxPrimarySearchPhraseLength);
        RuleFor(request => request.PackageQuantity)
            .GreaterThan(0)
            .When(request => request.PackageQuantity.HasValue);
        RuleFor(request => request)
            .Must(request => request.PackageQuantity.HasValue
                == !string.IsNullOrWhiteSpace(request.PackageUnit))
            .WithMessage("PackageQuantity and PackageUnit must be supplied together.");

        ConfigureCollection(request => request.AlternativeSearchPhrases);
        ConfigureCollection(request => request.RequiredKeywords);
        ConfigureCollection(request => request.ExcludedKeywords);
    }

    private void ConfigureCollection(
        System.Linq.Expressions.Expression<Func<TRequest, IEnumerable<string>>> expression)
    {
        RuleFor(expression)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(values => values.Count() <= ProductSearchConfiguration.MaxCollectionCount)
            .WithMessage(
                $"A search collection cannot contain more than {ProductSearchConfiguration.MaxCollectionCount} entries.");
        RuleForEach(expression)
            .NotEmpty()
            .MaximumLength(ProductSearchConfiguration.MaxEntryLength);
    }

    private static bool IsSupportedStatus(string? value) =>
        Enum.TryParse<ProductStatus>(value?.Trim(), true, out var status)
        && Enum.IsDefined(status);
}

public sealed class CreateProductValidator : ProductUpsertValidator<CreateProductCommand>;

public sealed class UpdateProductValidator : ProductUpsertValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
