using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Products.Commands;

/// <summary>Creates a product catalog entry.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateProductCommand : ProductUpsertDto, IRequest<Guid>;

public sealed class CreateProductHandler(IProductService productService)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken) =>
        productService.CreateAsync(request, cancellationToken);
}
