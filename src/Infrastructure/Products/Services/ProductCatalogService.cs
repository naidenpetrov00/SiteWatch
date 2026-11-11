using Application.ProductCatalog;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Products.Services;

public sealed class ProductCatalogService(ApplicationDbContext dbContext, IUser user)
    : IProductCatalogService
{
    public async Task<Guid> CreateCategoryAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var category = Category.Create(name);
        SetCreatedAudit(category);
        dbContext.Categories.Add(category);
        await SaveAsync("A Category with this name already exists.", cancellationToken);
        return category.Id;
    }

    public async Task UpdateCategoryAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.SingleOrDefaultAsync(
            current => current.Id == id,
            cancellationToken);
        Guard.Against.NotFound(id, category);
        category.Rename(name);
        SetModifiedAudit(category);

        var products = await dbContext.Products
            .Include(product => product.ProductType)
            .ThenInclude(productType => productType.Category)
            .Where(product => product.ProductType.CategoryId == id)
            .ToListAsync(cancellationToken);
        foreach (var product in products)
        {
            product.RefreshSearchIdentity();
        }

        await SaveAsync("A Category with this name already exists.", cancellationToken);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.SingleOrDefaultAsync(
            current => current.Id == id,
            cancellationToken);
        Guard.Against.NotFound(id, category);
        if (await dbContext.ProductTypes.AnyAsync(
                productType => productType.CategoryId == id,
                cancellationToken))
        {
            throw new ProductCatalogConflictException(
                "Delete the Category's Product Types before deleting the Category.");
        }

        dbContext.Categories.Remove(category);
        await SaveAsync("The Category is referenced and cannot be deleted.", cancellationToken);
    }

    public async Task<CategoryDto> GetCategoryAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.AsNoTracking().SingleOrDefaultAsync(
            current => current.Id == id,
            cancellationToken);
        Guard.Against.NotFound(id, category);
        return new CategoryDto(category.Id, category.Name);
    }

    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(
        CategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Categories.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var normalized = NormalizeSearch(request.SearchTerm);
        if (normalized.Length > 0)
        {
            query = query.Where(category => category.NormalizedName.Contains(normalized));
        }

        var filtered = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Select(category => new CategoryDto(category.Id, category.Name))
            .ToListAsync(cancellationToken);
        return new PagedResult<CategoryDto>(items, filtered, total);
    }

    public Task<Guid> CreateProductTypeAsync(
        Guid categoryId,
        string name,
        CancellationToken cancellationToken) =>
        CreateProductTypeAsync(categoryId, name, null, cancellationToken);

    public async Task<Guid> CreateProductTypeAsync(
        Guid categoryId,
        string name,
        string? description,
        CancellationToken cancellationToken)
    {
        var category = await LoadCategoryAsync(categoryId, cancellationToken);
        var productType = ProductType.Create(category, name, description);
        SetCreatedAudit(productType);
        dbContext.ProductTypes.Add(productType);
        await SaveAsync(
            "A Product Type with this name already exists in the Category.",
            cancellationToken);
        return productType.Id;
    }

    public Task UpdateProductTypeAsync(
        Guid id,
        Guid categoryId,
        string name,
        CancellationToken cancellationToken) =>
        UpdateProductTypeAsync(id, categoryId, name, null, cancellationToken);

    public async Task UpdateProductTypeAsync(
        Guid id,
        Guid categoryId,
        string name,
        string? description,
        CancellationToken cancellationToken)
    {
        var productType = await dbContext.ProductTypes
            .Include(current => current.Category)
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);
        Guard.Against.NotFound(id, productType);
        var category = await LoadCategoryAsync(categoryId, cancellationToken);
        productType.Update(category, name, description);
        SetModifiedAudit(productType);

        var products = await dbContext.Products
            .Include(product => product.ProductType)
            .ThenInclude(current => current.Category)
            .Where(product => product.ProductTypeId == id)
            .ToListAsync(cancellationToken);
        foreach (var product in products)
        {
            product.RefreshSearchIdentity();
        }

        await SaveAsync(
            "A Product Type with this name already exists in the Category.",
            cancellationToken);
    }

    public async Task DeleteProductTypeAsync(Guid id, CancellationToken cancellationToken)
    {
        var productType = await dbContext.ProductTypes.SingleOrDefaultAsync(
            current => current.Id == id,
            cancellationToken);
        Guard.Against.NotFound(id, productType);
        var referenced = await dbContext.Products.AnyAsync(
                product => product.ProductTypeId == id,
                cancellationToken)
            || await dbContext.ActivityProductRequirements.AnyAsync(
                requirement => requirement.ProductTypeId == id,
                cancellationToken)
            || await dbContext.OfferProductRequirements.AnyAsync(
                requirement => requirement.ProductTypeId == id,
                cancellationToken);
        if (referenced)
        {
            throw new ProductCatalogConflictException(
                "The Product Type is referenced and cannot be deleted.");
        }

        dbContext.ProductTypes.Remove(productType);
        await SaveAsync("The Product Type is referenced and cannot be deleted.", cancellationToken);
    }

    public async Task<ProductTypeDto> GetProductTypeAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var productType = await dbContext.ProductTypes
            .AsNoTracking()
            .Include(current => current.Category)
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);
        Guard.Against.NotFound(id, productType);
        return ToDto(productType);
    }

    public async Task<PagedResult<ProductTypeDto>> GetProductTypesAsync(
        ProductTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ProductTypes
            .AsNoTracking()
            .Include(productType => productType.Category)
            .AsQueryable();
        var total = await query.CountAsync(cancellationToken);
        if (request.CategoryId.HasValue)
        {
            query = query.Where(productType => productType.CategoryId == request.CategoryId.Value);
        }
        var normalized = NormalizeSearch(request.SearchTerm);
        if (normalized.Length > 0)
        {
            query = query.Where(productType => productType.NormalizedName.Contains(normalized));
        }

        var filtered = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderBy(productType => productType.Category.Name)
            .ThenBy(productType => productType.Name)
            .ThenBy(productType => productType.Id)
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<ProductTypeDto>(entities.Select(ToDto).ToList(), filtered, total);
    }

    public async Task<Guid> CreateProductBrandAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var productBrand = ProductBrand.Create(name);
        SetCreatedAudit(productBrand);
        dbContext.ProductBrands.Add(productBrand);
        await SaveAsync("A Product Brand with this name already exists.", cancellationToken);
        return productBrand.Id;
    }

    public async Task UpdateProductBrandAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken)
    {
        var productBrand = await dbContext.ProductBrands.SingleOrDefaultAsync(
            current => current.Id == id,
            cancellationToken);
        Guard.Against.NotFound(id, productBrand);
        productBrand.Rename(name);
        SetModifiedAudit(productBrand);

        var products = await dbContext.Products
            .Include(product => product.ProductBrand)
            .Include(product => product.ProductType)
            .ThenInclude(productType => productType.Category)
            .Where(product => product.ProductBrandId == id)
            .ToListAsync(cancellationToken);
        foreach (var product in products)
        {
            product.SynchronizeBrandName();
        }

        await SaveAsync("A Product Brand with this name already exists.", cancellationToken);
    }

    public async Task DeleteProductBrandAsync(Guid id, CancellationToken cancellationToken)
    {
        var productBrand = await dbContext.ProductBrands.SingleOrDefaultAsync(
            current => current.Id == id,
            cancellationToken);
        Guard.Against.NotFound(id, productBrand);
        if (await dbContext.Products.AnyAsync(
                product => product.ProductBrandId == id,
                cancellationToken))
        {
            throw new ProductCatalogConflictException(
                "The Product Brand is referenced and cannot be deleted.");
        }

        dbContext.ProductBrands.Remove(productBrand);
        await SaveAsync("The Product Brand is referenced and cannot be deleted.", cancellationToken);
    }

    public async Task<ProductBrandDto> GetProductBrandAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var productBrand = await dbContext.ProductBrands.AsNoTracking().SingleOrDefaultAsync(
            current => current.Id == id,
            cancellationToken);
        Guard.Against.NotFound(id, productBrand);
        return ToDto(productBrand);
    }

    public async Task<PagedResult<ProductBrandDto>> GetProductBrandsAsync(
        ProductBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ProductBrands.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var normalized = NormalizeSearch(request.SearchTerm);
        if (normalized.Length > 0)
        {
            query = query.Where(brand => brand.NormalizedName.Contains(normalized));
        }

        var filtered = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(brand => brand.Name)
            .ThenBy(brand => brand.Id)
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Select(brand => new ProductBrandDto(brand.Id, brand.Name))
            .ToListAsync(cancellationToken);
        return new PagedResult<ProductBrandDto>(items, filtered, total);
    }

    private async Task<Category> LoadCategoryAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.SingleOrDefaultAsync(
            current => current.Id == id,
            cancellationToken);
        Guard.Against.NotFound(id, category);
        return category;
    }

    private async Task SaveAsync(string conflictMessage, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new ProductCatalogConflictException(conflictMessage, exception);
        }
    }

    private void SetCreatedAudit(BaseAuditableEntity entity)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = GetUserId();
        entity.Created = now;
        entity.CreatedBy = userId;
        entity.LastModified = now;
        entity.LastModifiedBy = userId;
    }

    private void SetModifiedAudit(BaseAuditableEntity entity)
    {
        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = GetUserId();
    }

    private string GetUserId() => user.Id ?? throw new UnauthorizedAccessException();

    private static ProductTypeDto ToDto(ProductType productType) => new(
        productType.Id,
        productType.Name,
        productType.CategoryId,
        productType.Category.Name,
        productType.Description);

    private static ProductBrandDto ToDto(ProductBrand productBrand) =>
        new(productBrand.Id, productBrand.Name);

    private static string NormalizeSearch(string? value) => string.Join(
        " ",
        (value ?? string.Empty).Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
}
