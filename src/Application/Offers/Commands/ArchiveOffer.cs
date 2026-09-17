using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Commands;

/// <summary>Archives an offer while retaining its history.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ArchiveOfferCommand(Guid SiteId, Guid OfferId) : IRequest;

public sealed class ArchiveOfferValidator : AbstractValidator<ArchiveOfferCommand>
{
    public ArchiveOfferValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
    }
}

public sealed class ArchiveOfferHandler(IOfferService offerService)
    : IRequestHandler<ArchiveOfferCommand>
{
    public Task Handle(
        ArchiveOfferCommand request,
        CancellationToken cancellationToken) =>
        offerService.ArchiveAsync(
            request.SiteId,
            request.OfferId,
            cancellationToken);
}
