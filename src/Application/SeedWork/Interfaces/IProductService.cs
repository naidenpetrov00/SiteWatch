using Application.Products.Commands;

namespace Application.SeedWork.Interfaces;

public interface IProductService
{
    Task<Guid> CreateAsync(ProductUpsertDto request, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateProductCommand request, CancellationToken cancellationToken);
}
