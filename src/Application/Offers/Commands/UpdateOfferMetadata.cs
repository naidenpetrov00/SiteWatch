using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Offers.Commands;

/// <summary>Replaces the editable metadata of a draft offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateOfferMetadataCommand : IRequest
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public string? Title { get; init; }
    public string? Notes { get; init; }
}

public sealed class UpdateOfferMetadataValidator
    : AbstractValidator<UpdateOfferMetadataCommand>
{
    public UpdateOfferMetadataValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.Title).MaximumLength(Offer.MaxTitleLength);
        RuleFor(command => command.Notes).MaximumLength(Offer.MaxNotesLength);
    }
}

public sealed class UpdateOfferMetadataHandler(IOfferService offerService)
    : IRequestHandler<UpdateOfferMetadataCommand>
{
    public Task Handle(
        UpdateOfferMetadataCommand request,
        CancellationToken cancellationToken) =>
        offerService.UpdateMetadataAsync(request, cancellationToken);
}
