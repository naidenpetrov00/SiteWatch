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
    public decimal? PackageQuantity { get; private set; }
    public ProductPackageUnit? PackageUnit { get; private set; }
    public Guid ProductTypeId { get; private set; }
    public ProductType ProductType { get; private set; } = null!;
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
        decimal? packageQuantity,
        ProductPackageUnit? packageUnit,
        ProductType productType,
        ProductStatus status,
        ProductSearchConfiguration searchConfiguration)
    {
        var product = new Product { Id = Guid.NewGuid() };
        product.UpdateDetails(
            title,
            description,
            brand,
            model,
            packageQuantity,
            packageUnit,
            productType,
            status,
            searchConfiguration);
        return product;
    }

    public void UpdateDetails(
        string title,
        string? description,
        string? brand,
        string? model,
        decimal? packageQuantity,
        ProductPackageUnit? packageUnit,
        ProductType productType,
        ProductStatus status,
        ProductSearchConfiguration searchConfiguration)
    {
        ValidatePackagePair(packageQuantity, packageUnit);

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
        PackageQuantity = packageQuantity;
        PackageUnit = packageUnit;
        ProductType = Guard.Against.Null(productType);
        ProductTypeId = productType.Id;
        Status = status;
        SearchConfiguration = Guard.Against.Null(searchConfiguration);
        RefreshSearchIdentity();
    }

    public void RefreshSearchIdentity()
    {
        SearchIdentity = NormalizeSearchValue(
            string.Join(
                " ",
                new[]
                {
                    Title,
                    ProductType.Category.Name,
                    ProductType.Name,
                    Brand,
                    Model,
                    EffectivePrimarySearchPhrase
                }.Where(value => !string.IsNullOrWhiteSpace(value))));
    }

    private static void ValidatePackagePair(
        decimal? packageQuantity,
        ProductPackageUnit? packageUnit)
    {
        if (packageQuantity.HasValue != packageUnit.HasValue)
        {
            throw new ArgumentException(
                "Package quantity and unit must be supplied together.");
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
