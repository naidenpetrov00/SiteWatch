using Application.ProductCatalog;
using Application.SeedWork.Models;

namespace Application.SeedWork.Interfaces;

public interface IProductCatalogService
{
    Task<Guid> CreateCategoryAsync(string name, CancellationToken cancellationToken);
    Task UpdateCategoryAsync(Guid id, string name, CancellationToken cancellationToken);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<CategoryDto> GetCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(
        CategoriesQuery request,
        CancellationToken cancellationToken);
    Task<Guid> CreateProductTypeAsync(
        Guid categoryId,
        string name,
        CancellationToken cancellationToken);
    Task UpdateProductTypeAsync(
        Guid id,
        Guid categoryId,
        string name,
        CancellationToken cancellationToken);
    Task DeleteProductTypeAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductTypeDto> GetProductTypeAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<ProductTypeDto>> GetProductTypesAsync(
        ProductTypesQuery request,
        CancellationToken cancellationToken);
}
