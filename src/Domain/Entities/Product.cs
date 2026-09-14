using System.ComponentModel.DataAnnotations.Schema;
using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Product : BaseAuditableEntity, IHasNumberId, IAgregateRoot
{
    private Product()
    {
    }

    public int NumberId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public string? ExternalIdentifierType { get; private set; }
    public string? ExternalIdentifier { get; private set; }
    public decimal? PackageQuantity { get; private set; }
    public string? PackageUnit { get; private set; }
    public string Category { get; private set; } = null!;
    public ProductStatus Status { get; private set; }
    public ProductSearchConfiguration SearchConfiguration { get; private set; } = null!;
    public string SearchIdentity { get; private set; } = null!;

    [NotMapped]
    public string EffectivePrimarySearchPhrase =>
        SearchConfiguration.ResolvePrimarySearchPhrase(Title, Brand, Model);

    public static Product Create(
        string title,
        string? description,
        string? brand,
        string? model,
        string? externalIdentifierType,
        string? externalIdentifier,
        decimal? packageQuantity,
        string? packageUnit,
        string category,
        ProductStatus status,
        ProductSearchConfiguration searchConfiguration)
    {
        var product = new Product { Id = Guid.NewGuid() };
        product.UpdateDetails(
            title,
            description,
            brand,
            model,
            externalIdentifierType,
            externalIdentifier,
            packageQuantity,
            packageUnit,
            category,
            status,
            searchConfiguration);
        return product;
    }

    public void UpdateDetails(
        string title,
        string? description,
        string? brand,
        string? model,
        string? externalIdentifierType,
        string? externalIdentifier,
        decimal? packageQuantity,
        string? packageUnit,
        string category,
        ProductStatus status,
        ProductSearchConfiguration searchConfiguration)
    {
        ValidatePair(externalIdentifierType, externalIdentifier, "external identifier");
        ValidatePair(packageQuantity, packageUnit, "package quantity and unit");

        if (packageQuantity is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(packageQuantity),
                packageQuantity,
                "Package quantity must be greater than zero.");
        }

        Title = NormalizeRequired(title, nameof(title));
        Description = NormalizeOptional(description);
        Brand = NormalizeOptional(brand);
        Model = NormalizeOptional(model);
        ExternalIdentifierType = NormalizeOptional(externalIdentifierType);
        ExternalIdentifier = NormalizeOptional(externalIdentifier);
        PackageQuantity = packageQuantity;
        PackageUnit = NormalizeOptional(packageUnit);
        Category = NormalizeRequired(category, nameof(category));
        Status = status;
        SearchConfiguration = Guard.Against.Null(searchConfiguration);
        RefreshSearchIdentity();
    }

    private void RefreshSearchIdentity()
    {
        SearchIdentity = NormalizeSearchValue(
            string.Join(
                " ",
                new[]
                {
                    Title,
                    Category,
                    Brand,
                    Model,
                    ExternalIdentifierType,
                    ExternalIdentifier,
                    EffectivePrimarySearchPhrase
                }.Where(value => !string.IsNullOrWhiteSpace(value))));
    }

    private static void ValidatePair<T>(T? first, string? second, string fieldName)
    {
        var hasFirst = first is not null;
        var hasSecond = !string.IsNullOrWhiteSpace(second);
        if (hasFirst != hasSecond)
        {
            throw new ArgumentException($"Both values for {fieldName} must be supplied together.");
        }
    }

    private static string NormalizeRequired(string value, string parameterName) =>
        string.Join(
            " ",
            Guard.Against.NullOrWhiteSpace(value, parameterName)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string NormalizeSearchValue(string value) => value.ToUpperInvariant();
}
