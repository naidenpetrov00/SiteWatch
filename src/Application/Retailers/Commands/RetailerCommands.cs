using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Retailers.Commands;

/// <summary>Creates an active retailer storefront.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateRetailerCommand : RetailerUpsertDto, IRequest<Guid>;

public sealed class CreateRetailerHandler(IRetailerService retailerService)
    : IRequestHandler<CreateRetailerCommand, Guid>
{
    public Task<Guid> Handle(
        CreateRetailerCommand request,
        CancellationToken cancellationToken) =>
        retailerService.CreateAsync(request, cancellationToken);
}

/// <summary>Updates the editable storefront and legal-company reference of a retailer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateRetailerCommand : RetailerUpsertDto, IRequest
{
    public Guid Id { get; set; }
}

public sealed class UpdateRetailerHandler(IRetailerService retailerService)
    : IRequestHandler<UpdateRetailerCommand>
{
    public Task Handle(
        UpdateRetailerCommand request,
        CancellationToken cancellationToken) =>
        retailerService.UpdateAsync(request, cancellationToken);
}

/// <summary>Activates or deactivates a retailer without deleting historical identity.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record SetRetailerActiveCommand(Guid Id, bool IsActive) : IRequest;

public sealed class SetRetailerActiveHandler(IRetailerService retailerService)
    : IRequestHandler<SetRetailerActiveCommand>
{
    public Task Handle(
        SetRetailerActiveCommand request,
        CancellationToken cancellationToken) =>
        retailerService.SetActiveAsync(request.Id, request.IsActive, cancellationToken);
}
