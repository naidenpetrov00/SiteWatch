using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;

namespace Application.RetailerListings;

/// <summary>Creates one stable Product–Retailer listing.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateRetailerListingCommand : IRequest<RetailerListingDto>
{
    public Guid ProductId { get; init; }
    public Guid RetailerId { get; init; }
    public string? ProductUrl { get; init; }
    public string? RetailerProductCode { get; init; }
    public decimal? Amount { get; init; }
    public string? Basis { get; init; }
}

/// <summary>Updates listing metadata and optionally records a new manual price.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateRetailerListingCommand : IRequest<RetailerListingDto>
{
    public Guid ListingId { get; set; }
    public string? ProductUrl { get; init; }
    public string? RetailerProductCode { get; init; }
    public decimal? Amount { get; init; }
    public string? Basis { get; init; }
}

/// <summary>Activates or deactivates a retained retailer listing.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record SetRetailerListingActiveCommand(
    Guid ListingId,
    bool IsActive) : IRequest<RetailerListingDto>;

public sealed class CreateRetailerListingValidator
    : AbstractValidator<CreateRetailerListingCommand>
{
    public CreateRetailerListingValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.RetailerId).NotEmpty();
        RetailerListingValidation.AddRules(this);
    }
}

public sealed class UpdateRetailerListingValidator
    : AbstractValidator<UpdateRetailerListingCommand>
{
    public UpdateRetailerListingValidator()
    {
        RuleFor(command => command.ListingId).NotEmpty();
        RetailerListingValidation.AddRules(this);
    }
}

public sealed class SetRetailerListingActiveValidator
    : AbstractValidator<SetRetailerListingActiveCommand>
{
    public SetRetailerListingActiveValidator()
    {
        RuleFor(command => command.ListingId).NotEmpty();
    }
}

public sealed class CreateRetailerListingHandler(IRetailerListingService service)
    : IRequestHandler<CreateRetailerListingCommand, RetailerListingDto>
{
    public Task<RetailerListingDto> Handle(
        CreateRetailerListingCommand request,
        CancellationToken cancellationToken) =>
        service.CreateAsync(request, cancellationToken);
}

public sealed class UpdateRetailerListingHandler(IRetailerListingService service)
    : IRequestHandler<UpdateRetailerListingCommand, RetailerListingDto>
{
    public Task<RetailerListingDto> Handle(
        UpdateRetailerListingCommand request,
        CancellationToken cancellationToken) =>
        service.UpdateAsync(request, cancellationToken);
}

public sealed class SetRetailerListingActiveHandler(IRetailerListingService service)
    : IRequestHandler<SetRetailerListingActiveCommand, RetailerListingDto>
{
    public Task<RetailerListingDto> Handle(
        SetRetailerListingActiveCommand request,
        CancellationToken cancellationToken) =>
        service.SetActiveAsync(
            request.ListingId,
            request.IsActive,
            cancellationToken);
}

internal static class RetailerListingValidation
{
    private const decimal MaximumAmount = 9999999999999999.99m;

    internal static void AddRules<TCommand>(AbstractValidator<TCommand> validator)
        where TCommand : class
    {
        validator.RuleFor(command => ReadProductUrl(command))
            .MaximumLength(RetailerListing.MaxProductUrlLength)
            .Must(BeValidOptionalProductUrl)
            .WithName(nameof(CreateRetailerListingCommand.ProductUrl))
            .WithMessage(
                "ProductUrl must be an absolute HTTP or HTTPS URL without credentials or a fragment.");
        validator.RuleFor(command => ReadRetailerProductCode(command))
            .MaximumLength(RetailerListing.MaxRetailerProductCodeLength)
            .WithName(nameof(CreateRetailerListingCommand.RetailerProductCode));
        validator.RuleFor(command => ReadAmount(command))
            .GreaterThan(0m)
            .LessThanOrEqualTo(MaximumAmount)
            .Must(amount => !amount.HasValue || decimal.Round(amount.Value, 2) == amount.Value)
            .When(command => ReadAmount(command).HasValue)
            .WithName(nameof(CreateRetailerListingCommand.Amount))
            .WithMessage("Amount must be positive and have at most two decimal places.");
        validator.RuleFor(command => ReadBasis(command))
            .Must((command, basis) =>
                ReadAmount(command).HasValue
                    ? PriceBasisCodes.TryParse(basis, out _)
                    : string.IsNullOrWhiteSpace(basis))
            .WithName(nameof(CreateRetailerListingCommand.Basis))
            .WithMessage("Basis must be item or package when an amount is supplied.");
    }

    private static string? ReadProductUrl<TCommand>(TCommand command) =>
        command switch
        {
            CreateRetailerListingCommand create => create.ProductUrl,
            UpdateRetailerListingCommand update => update.ProductUrl,
            _ => null
        };

    private static string? ReadRetailerProductCode<TCommand>(TCommand command) =>
        command switch
        {
            CreateRetailerListingCommand create => create.RetailerProductCode,
            UpdateRetailerListingCommand update => update.RetailerProductCode,
            _ => null
        };

    private static decimal? ReadAmount<TCommand>(TCommand command) =>
        command switch
        {
            CreateRetailerListingCommand create => create.Amount,
            UpdateRetailerListingCommand update => update.Amount,
            _ => null
        };

    private static string? ReadBasis<TCommand>(TCommand command) =>
        command switch
        {
            CreateRetailerListingCommand create => create.Basis,
            UpdateRetailerListingCommand update => update.Basis,
            _ => null
        };

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
