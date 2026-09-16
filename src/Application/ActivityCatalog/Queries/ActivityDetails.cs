using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ActivityCatalog.Queries;

/// <summary>Loads the complete editable details of one activity.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ActivityDetailsQuery(Guid ActivityId) : IRequest<ActivityDetailsDto>;

/// <summary>Represents the editable details of an activity.</summary>
public sealed record ActivityDetailsDto(
    Guid Id,
    int NumberId,
    string Name,
    string? Description,
    string Status,
    Guid? ParentFolderId,
    int SortOrder,
    IReadOnlyList<ActivityRequirementSectionDto> RequirementSections)
{
    public ActivityDetailsDto(
        Guid id,
        int numberId,
        string name,
        string? description,
        string status,
        Guid? parentFolderId,
        int sortOrder)
        : this(
            id,
            numberId,
            name,
            description,
            status,
            parentFolderId,
            sortOrder,
            [])
    {
    }
}

/// <summary>Represents one measurement-based requirement section.</summary>
public sealed record ActivityRequirementSectionDto(
    Guid Id,
    string? Name,
    decimal BasisQuantity,
    string MeasurementUnit,
    int SortOrder,
    IReadOnlyList<ActivityProductRequirementDto> ProductRequirements);

/// <summary>Represents one Product Catalog item required by a section.</summary>
public sealed record ActivityProductRequirementDto(
    Guid Id,
    Guid ProductId,
    int ProductNumberId,
    string ProductTitle,
    string ProductStatus,
    string? Brand,
    string? Model,
    decimal? PackageQuantity,
    string? PackageUnit,
    decimal Quantity,
    bool IsRequired,
    string QuantityBehavior,
    string? Notes,
    int SortOrder);

public sealed class ActivityDetailsQueryValidator : AbstractValidator<ActivityDetailsQuery>
{
    public ActivityDetailsQueryValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
    }
}

public sealed class ActivityDetailsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ActivityDetailsQuery, ActivityDetailsDto>
{
    public async Task<ActivityDetailsDto> Handle(
        ActivityDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var activity = await dbContext.ActivityCatalogNodes
            .OfType<Activity>()
            .AsNoTracking()
            .Include(activity => activity.RequirementSections)
            .ThenInclude(section => section.ProductRequirements)
            .ThenInclude(requirement => requirement.Product)
            .SingleOrDefaultAsync(
                activity => activity.Id == request.ActivityId,
                cancellationToken);

        Guard.Against.NotFound(request.ActivityId, activity);

        return new ActivityDetailsDto(
            activity.Id,
            activity.NumberId,
            activity.Name,
            activity.Description,
            activity.Status.ToString(),
            activity.ParentFolderId,
            activity.SortOrder,
            activity.RequirementSections
                .OrderBy(section => section.SortOrder)
                .ThenBy(section => section.Id)
                .Select(section => new ActivityRequirementSectionDto(
                    section.Id,
                    section.Name,
                    section.BasisQuantity,
                    section.MeasurementUnit.ToCode(),
                    section.SortOrder,
                    section.ProductRequirements
                        .OrderBy(requirement => requirement.SortOrder)
                        .ThenBy(requirement => requirement.Id)
                        .Select(requirement => new ActivityProductRequirementDto(
                            requirement.Id,
                            requirement.ProductId,
                            requirement.Product.NumberId,
                            requirement.Product.Title,
                            requirement.Product.Status.ToString(),
                            requirement.Product.Brand,
                            requirement.Product.Model,
                            requirement.Product.PackageQuantity,
                            requirement.Product.PackageUnit?.ToCode(),
                            requirement.Quantity,
                            requirement.IsRequired,
                            requirement.QuantityBehavior.ToCode(),
                            requirement.Notes,
                            requirement.SortOrder))
                        .ToList()))
                .ToList());
    }
}
