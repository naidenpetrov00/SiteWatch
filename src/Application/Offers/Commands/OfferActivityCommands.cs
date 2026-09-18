using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Commands;

/// <summary>Supplies one requested measurement for a source activity section.</summary>
public sealed record OfferSectionMeasurementInput(
    Guid SectionId,
    decimal RequestedMeasurement);

/// <summary>Adds an active Activity Catalog item and its immutable requirement snapshot.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record AddOfferActivityCommand : IRequest<Guid>
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public Guid ActivityId { get; init; }
    public IReadOnlyList<OfferSectionMeasurementInput> SectionMeasurements { get; init; } = [];
}

public sealed class AddOfferActivityHandler(IOfferService offerService)
    : IRequestHandler<AddOfferActivityCommand, Guid>
{
    public Task<Guid> Handle(
        AddOfferActivityCommand request,
        CancellationToken cancellationToken) =>
        offerService.AddActivityAsync(request, cancellationToken);
}

/// <summary>Removes a selected activity snapshot from a draft Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RemoveOfferActivityCommand(
    Guid SiteId,
    Guid OfferId,
    Guid OfferActivityId) : IRequest;

public sealed class RemoveOfferActivityHandler(IOfferService offerService)
    : IRequestHandler<RemoveOfferActivityCommand>
{
    public Task Handle(
        RemoveOfferActivityCommand request,
        CancellationToken cancellationToken) =>
        offerService.RemoveActivityAsync(
            request.SiteId,
            request.OfferId,
            request.OfferActivityId,
            cancellationToken);
}

/// <summary>Replaces every requested measurement for one selected Offer activity.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateOfferActivityMeasurementsCommand : IRequest
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public Guid OfferActivityId { get; set; }
    public IReadOnlyList<OfferSectionMeasurementInput> SectionMeasurements { get; init; } = [];
}

public sealed class UpdateOfferActivityMeasurementsHandler(IOfferService offerService)
    : IRequestHandler<UpdateOfferActivityMeasurementsCommand>
{
    public Task Handle(
        UpdateOfferActivityMeasurementsCommand request,
        CancellationToken cancellationToken) =>
        offerService.UpdateActivityMeasurementsAsync(request, cancellationToken);
}

public sealed class OfferSectionMeasurementInputValidator
    : AbstractValidator<OfferSectionMeasurementInput>
{
    public OfferSectionMeasurementInputValidator()
    {
        RuleFor(input => input.SectionId).NotEmpty();
        RuleFor(input => input.RequestedMeasurement)
            .GreaterThan(0m)
            .PrecisionScale(18, 4, false);
    }
}

public sealed class AddOfferActivityValidator : AbstractValidator<AddOfferActivityCommand>
{
    public AddOfferActivityValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.ActivityId).NotEmpty();
        RuleForEach(command => command.SectionMeasurements)
            .SetValidator(new OfferSectionMeasurementInputValidator());
        RuleFor(command => command.SectionMeasurements)
            .Must(HaveUniqueSectionIds)
            .WithMessage("Each activity section can be supplied only once.");
    }

    private static bool HaveUniqueSectionIds(
        IReadOnlyList<OfferSectionMeasurementInput>? measurements) =>
        measurements is not null
        && measurements.Select(measurement => measurement.SectionId).Distinct().Count()
        == measurements.Count;
}

public sealed class RemoveOfferActivityValidator
    : AbstractValidator<RemoveOfferActivityCommand>
{
    public RemoveOfferActivityValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.OfferActivityId).NotEmpty();
    }
}

public sealed class UpdateOfferActivityMeasurementsValidator
    : AbstractValidator<UpdateOfferActivityMeasurementsCommand>
{
    public UpdateOfferActivityMeasurementsValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.OfferActivityId).NotEmpty();
        RuleForEach(command => command.SectionMeasurements)
            .SetValidator(new OfferSectionMeasurementInputValidator());
        RuleFor(command => command.SectionMeasurements)
            .Must(measurements =>
                measurements is not null
                && measurements.Select(measurement => measurement.SectionId).Distinct().Count()
                == measurements.Count)
            .WithMessage("Each Offer activity section can be supplied only once.");
    }
}
