using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Queries;

/// <summary>Loads one product by its catalog identifier.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ProductByIdQuery : IRequest<ProductDetailsDto>
{
    public Guid ProductId { get; init; }
}

public sealed class ProductByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ProductByIdQuery, ProductDetailsDto>
{
    public async Task<ProductDetailsDto> Handle(
        ProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == request.ProductId, cancellationToken);

        Guard.Against.NotFound(request.ProductId, product);

        return ProductDetailsDto.From(product);
    }
}
