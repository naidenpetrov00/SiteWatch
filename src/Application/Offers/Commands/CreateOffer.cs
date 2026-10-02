using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Commands;

/// <summary>Creates a persisted draft offer for a site.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateOfferCommand : IRequest<Guid>
{
    public Guid SiteId { get; set; }
}

public sealed class CreateOfferValidator : AbstractValidator<CreateOfferCommand>
{
    public CreateOfferValidator() => RuleFor(command => command.SiteId).NotEmpty();
}

public sealed class CreateOfferHandler(IOfferService offerService)
    : IRequestHandler<CreateOfferCommand, Guid>
{
    public Task<Guid> Handle(
        CreateOfferCommand request,
        CancellationToken cancellationToken) =>
        offerService.CreateAsync(request.SiteId, cancellationToken);
}
