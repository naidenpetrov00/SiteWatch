using Application.Products.Commands;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Products.Services;

public sealed class ProductService(ApplicationDbContext dbContext) : IProductService
{
    public async Task<Guid> CreateAsync(
        ProductUpsertDto request,
        CancellationToken cancellationToken)
    {
        var product = CreateProduct(request);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return product.Id;
    }

    public async Task UpdateAsync(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .SingleOrDefaultAsync(product => product.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, product);

        product.UpdateDetails(
            request.Title,
            request.Description,
            request.Brand,
            request.Model,
            request.PackageQuantity,
            ParsePackageUnit(request.PackageUnit),
            ParseCategory(request.Category),
            ParseStatus(request.Status),
            CreateSearchConfiguration(request));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Product CreateProduct(ProductUpsertDto request) =>
        Product.Create(
            request.Title,
            request.Description,
            request.Brand,
            request.Model,
            request.PackageQuantity,
            ParsePackageUnit(request.PackageUnit),
            ParseCategory(request.Category),
            ParseStatus(request.Status),
            CreateSearchConfiguration(request));

    private static ProductSearchConfiguration CreateSearchConfiguration(ProductUpsertDto request) =>
        ProductSearchConfiguration.Create(
            request.PrimarySearchPhrase,
            request.AlternativeSearchPhrases,
            request.RequiredKeywords,
            request.ExcludedKeywords);

    private static ProductPackageUnit? ParsePackageUnit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (ProductPackageUnitCodes.TryParse(value, out var unit))
        {
            return unit;
        }

        throw new ArgumentException($"Unsupported product package unit '{value}'.", nameof(value));
    }

    private static ProductStatus ParseStatus(string value)
    {
        var normalizedValue = Guard.Against.NullOrWhiteSpace(value).Trim();
        if (!Enum.TryParse<ProductStatus>(normalizedValue, true, out var status)
            || !Enum.IsDefined(status))
        {
            throw new ArgumentException($"Unsupported product status '{value}'.", nameof(value));
        }

        return status;
    }

    private static ProductCategory ParseCategory(string value)
    {
        if (ProductCategoryCodes.TryParse(value, out var category))
        {
            return category;
        }

        throw new ArgumentException($"Unsupported product category '{value}'.", nameof(value));
    }
}
