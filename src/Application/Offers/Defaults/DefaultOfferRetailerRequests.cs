using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Offers.Defaults;

/// <summary>Loads the ordered global retailer defaults for new Offers.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DefaultOfferRetailersQuery
    : IRequest<IReadOnlyList<DefaultOfferRetailerDto>>;

public sealed class DefaultOfferRetailersQueryHandler(
    IDefaultOfferRetailerService service)
    : IRequestHandler<DefaultOfferRetailersQuery, IReadOnlyList<DefaultOfferRetailerDto>>
{
    public Task<IReadOnlyList<DefaultOfferRetailerDto>> Handle(
        DefaultOfferRetailersQuery request,
        CancellationToken cancellationToken) =>
        service.GetAsync(cancellationToken);
}

/// <summary>Replaces the complete ordered global retailer defaults for new Offers.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ReplaceDefaultOfferRetailersCommand
    : IRequest<IReadOnlyList<DefaultOfferRetailerDto>>
{
    public IReadOnlyList<Guid> RetailerIds { get; init; } = [];
}

public sealed class ReplaceDefaultOfferRetailersValidator
    : AbstractValidator<ReplaceDefaultOfferRetailersCommand>
{
    public ReplaceDefaultOfferRetailersValidator()
    {
        RuleFor(request => request.RetailerIds).NotNull();
        RuleFor(request => request.RetailerIds)
            .Must(ids => ids is not null && ids.Count <= DefaultOfferRetailer.MaximumCount)
            .WithMessage(
                $"RetailerIds must contain at most {DefaultOfferRetailer.MaximumCount} retailers.");
        RuleForEach(request => request.RetailerIds).NotEmpty();
        RuleFor(request => request.RetailerIds)
            .Must(ids => ids is not null && ids.Distinct().Count() == ids.Count)
            .WithMessage("RetailerIds cannot contain duplicates.");
    }
}

public sealed class ReplaceDefaultOfferRetailersHandler(
    IDefaultOfferRetailerService service)
    : IRequestHandler<
        ReplaceDefaultOfferRetailersCommand,
        IReadOnlyList<DefaultOfferRetailerDto>>
{
    public Task<IReadOnlyList<DefaultOfferRetailerDto>> Handle(
        ReplaceDefaultOfferRetailersCommand request,
        CancellationToken cancellationToken) =>
        service.ReplaceAsync(request.RetailerIds, cancellationToken);
}
