using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Products.Commands;

/// <summary>Replaces the editable details of an existing product.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateProductCommand : ProductUpsertDto, IRequest
{
    public Guid Id { get; set; }
}

public sealed class UpdateProductHandler(IProductService productService)
    : IRequestHandler<UpdateProductCommand>
{
    public Task Handle(UpdateProductCommand request, CancellationToken cancellationToken) =>
        productService.UpdateAsync(request, cancellationToken);
}
