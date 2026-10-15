using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;

namespace Application.Offers.Pricing;

/// <summary>Adds an active Retailer to a draft Offer comparison matrix.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record AddOfferRetailerCommand : IRequest<OfferPricingMatrixDto>
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public Guid RetailerId { get; init; }
}

/// <summary>Removes an unused Retailer from a draft Offer comparison matrix.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RemoveOfferRetailerCommand(
    Guid SiteId,
    Guid OfferId,
    Guid RetailerId) : IRequest;

/// <summary>Records the current manually observed EUR price for an Offer cell.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RecordManualRetailerPriceCommand : IRequest<OfferRetailerPriceCellDto>
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public Guid OfferProductLineId { get; set; }
    public Guid RetailerId { get; set; }
    public decimal Amount { get; init; }
    public string Basis { get; init; } = string.Empty;
    public string? ProductUrl { get; init; }
    public string? RetailerProductCode { get; init; }
}

/// <summary>Snapshots the latest selected retailer price for one Offer Product.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record SelectOfferProductPriceCommand : IRequest<OfferPricingProductRowDto>
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public Guid OfferProductLineId { get; set; }
    public Guid RetailerId { get; init; }
    public Guid ObservationId { get; init; }
}

/// <summary>Clears the selected price snapshot for one draft Offer Product.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ClearOfferProductPriceCommand(
    Guid SiteId,
    Guid OfferId,
    Guid OfferProductLineId) : IRequest;

/// <summary>Replaces both category-wide percentage discounts on a draft Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateOfferDiscountsCommand : IRequest
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public decimal ActivityDiscountPercentage { get; init; }
    public decimal ProductDiscountPercentage { get; init; }
}

public sealed class AddOfferRetailerValidator : AbstractValidator<AddOfferRetailerCommand>
{
    public AddOfferRetailerValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.RetailerId).NotEmpty();
    }
}

public sealed class RemoveOfferRetailerValidator : AbstractValidator<RemoveOfferRetailerCommand>
{
    public RemoveOfferRetailerValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.RetailerId).NotEmpty();
    }
}

public sealed class RecordManualRetailerPriceValidator
    : AbstractValidator<RecordManualRetailerPriceCommand>
{
    private const decimal MaximumAmount = 9999999999999999.99m;

    public RecordManualRetailerPriceValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.OfferProductLineId).NotEmpty();
        RuleFor(command => command.RetailerId).NotEmpty();
        RuleFor(command => command.Amount)
            .GreaterThan(0m)
            .LessThanOrEqualTo(MaximumAmount)
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("Amount must have at most two decimal places.");
        RuleFor(command => command.Basis)
            .Must(value => PriceBasisCodes.TryParse(value, out _))
            .WithMessage("Basis must be item or package.");
        RuleFor(command => command.ProductUrl)
            .MaximumLength(RetailerListing.MaxProductUrlLength)
            .Must(BeValidOptionalProductUrl)
            .WithMessage(
                "ProductUrl must be an absolute HTTP or HTTPS URL without credentials or a fragment.");
        RuleFor(command => command.RetailerProductCode)
            .MaximumLength(RetailerListing.MaxRetailerProductCodeLength);
    }

    private static bool BeValidOptionalProductUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrWhiteSpace(uri.Host)
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Fragment);
    }
}

public sealed class SelectOfferProductPriceValidator
    : AbstractValidator<SelectOfferProductPriceCommand>
{
    public SelectOfferProductPriceValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.OfferProductLineId).NotEmpty();
        RuleFor(command => command.RetailerId).NotEmpty();
        RuleFor(command => command.ObservationId).NotEmpty();
    }
}

public sealed class ClearOfferProductPriceValidator
    : AbstractValidator<ClearOfferProductPriceCommand>
{
    public ClearOfferProductPriceValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.OfferProductLineId).NotEmpty();
    }
}

public sealed class UpdateOfferDiscountsValidator
    : AbstractValidator<UpdateOfferDiscountsCommand>
{
    public UpdateOfferDiscountsValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.ActivityDiscountPercentage)
            .InclusiveBetween(0m, Offer.MaximumDiscountPercentage)
            .PrecisionScale(5, 2, false);
        RuleFor(command => command.ProductDiscountPercentage)
            .InclusiveBetween(0m, Offer.MaximumDiscountPercentage)
            .PrecisionScale(5, 2, false);
    }
}

public sealed class AddOfferRetailerHandler(IOfferPricingService service)
    : IRequestHandler<AddOfferRetailerCommand, OfferPricingMatrixDto>
{
    public Task<OfferPricingMatrixDto> Handle(
        AddOfferRetailerCommand request,
        CancellationToken cancellationToken) =>
        service.AddRetailerAsync(request, cancellationToken);
}

public sealed class RemoveOfferRetailerHandler(IOfferPricingService service)
    : IRequestHandler<RemoveOfferRetailerCommand>
{
    public Task Handle(RemoveOfferRetailerCommand request, CancellationToken cancellationToken) =>
        service.RemoveRetailerAsync(
            request.SiteId,
            request.OfferId,
            request.RetailerId,
            cancellationToken);
}

public sealed class RecordManualRetailerPriceHandler(IOfferPricingService service)
    : IRequestHandler<RecordManualRetailerPriceCommand, OfferRetailerPriceCellDto>
{
    public Task<OfferRetailerPriceCellDto> Handle(
        RecordManualRetailerPriceCommand request,
        CancellationToken cancellationToken) =>
        service.RecordManualPriceAsync(request, cancellationToken);
}

public sealed class SelectOfferProductPriceHandler(IOfferPricingService service)
    : IRequestHandler<SelectOfferProductPriceCommand, OfferPricingProductRowDto>
{
    public Task<OfferPricingProductRowDto> Handle(
        SelectOfferProductPriceCommand request,
        CancellationToken cancellationToken) =>
        service.SelectPriceAsync(request, cancellationToken);
}

public sealed class ClearOfferProductPriceHandler(IOfferPricingService service)
    : IRequestHandler<ClearOfferProductPriceCommand>
{
    public Task Handle(
        ClearOfferProductPriceCommand request,
        CancellationToken cancellationToken) =>
        service.ClearSelectionAsync(
            request.SiteId,
            request.OfferId,
            request.OfferProductLineId,
            cancellationToken);
}

public sealed class UpdateOfferDiscountsHandler(IOfferPricingService service)
    : IRequestHandler<UpdateOfferDiscountsCommand>
{
    public Task Handle(
        UpdateOfferDiscountsCommand request,
        CancellationToken cancellationToken) =>
        service.UpdateDiscountsAsync(request, cancellationToken);
}
