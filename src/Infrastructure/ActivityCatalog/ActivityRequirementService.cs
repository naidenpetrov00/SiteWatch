using System.Data;
using Application.ActivityCatalog;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.ActivityCatalog;

public sealed class ActivityRequirementService(ApplicationDbContext dbContext)
    : IActivityRequirementService
{
    public Task<Guid> CreateSectionAsync(
        Guid activityId,
        string? name,
        decimal basisQuantity,
        ActivityMeasurementUnit measurementUnit,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadActivityAsync(activityId, cancellationToken);
                EnsureEditable(activity);
                var sortOrder = await dbContext.ActivityRequirementSections.CountAsync(
                    section => section.ActivityId == activityId,
                    cancellationToken);
                var section = activity.AddRequirementSection(
                    name,
                    basisQuantity,
                    measurementUnit,
                    sortOrder);
                dbContext.ActivityRequirementSections.Add(section);
                return section.Id;
            },
            "The requirement section could not be created because the activity changed.",
            cancellationToken);

    public Task UpdateSectionAsync(
        Guid activityId,
        Guid sectionId,
        string? name,
        decimal basisQuantity,
        ActivityMeasurementUnit measurementUnit,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadActivityAsync(activityId, cancellationToken);
                EnsureEditable(activity);
                var section = await LoadSectionAsync(
                    activityId,
                    sectionId,
                    cancellationToken);
                section.UpdateDetails(name, basisQuantity, measurementUnit);
            },
            "The requirement section could not be updated because the activity changed.",
            cancellationToken);

    public Task MoveSectionAsync(
        Guid activityId,
        Guid sectionId,
        int targetIndex,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadActivityAsync(activityId, cancellationToken);
                EnsureEditable(activity);
                var sections = await dbContext.ActivityRequirementSections
                    .Where(section => section.ActivityId == activityId)
                    .OrderBy(section => section.SortOrder)
                    .ThenBy(section => section.Id)
                    .ToListAsync(cancellationToken);
                var section = sections.SingleOrDefault(section => section.Id == sectionId);
                Guard.Against.NotFound(sectionId, section);

                sections.Remove(section);
                EnsureTargetIndex(targetIndex, sections.Count);
                sections.Insert(targetIndex, section);
                RenumberSections(sections);
            },
            "The requirement section could not be moved because the activity changed.",
            cancellationToken);

    public Task DeleteSectionAsync(
        Guid activityId,
        Guid sectionId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await dbContext.ActivityCatalogNodes
                    .OfType<Activity>()
                    .Include(current => current.RequirementSections)
                    .SingleOrDefaultAsync(current => current.Id == activityId, cancellationToken);
                Guard.Against.NotFound(activityId, activity);
                EnsureEditable(activity);

                var section = activity.RequirementSections
                    .SingleOrDefault(current => current.Id == sectionId);
                Guard.Against.NotFound(sectionId, section);
                activity.RemoveRequirementSection(section);
                RenumberSections(
                    activity.RequirementSections
                        .OrderBy(current => current.SortOrder)
                        .ThenBy(current => current.Id)
                        .ToList());
                dbContext.ActivityRequirementSections.Remove(section);
            },
            "The requirement section could not be deleted because the activity changed.",
            cancellationToken);

    public Task<Guid> CreateProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid productId,
        decimal quantity,
        bool isRequired,
        ProductQuantityBehavior quantityBehavior,
        string? notes,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadActivityAsync(activityId, cancellationToken);
                EnsureEditable(activity);
                var section = await dbContext.ActivityRequirementSections
                    .Include(current => current.ProductRequirements)
                    .SingleOrDefaultAsync(
                        current => current.Id == sectionId
                            && current.ActivityId == activityId,
                        cancellationToken);
                Guard.Against.NotFound(sectionId, section);

                var product = await dbContext.Products.SingleOrDefaultAsync(
                    current => current.Id == productId,
                    cancellationToken);
                Guard.Against.NotFound(productId, product);
                if (product.Status != ProductStatus.Active)
                {
                    throw new ActivityCatalogConflictException(
                        "Only active products can be newly assigned to an activity.");
                }
                if (section.ProductRequirements.Any(
                    requirement => requirement.ProductId == productId))
                {
                    throw new ActivityCatalogConflictException(
                        "This product is already assigned to the requirement section.");
                }

                var requirement = section.AddProductRequirement(
                    product,
                    quantity,
                    isRequired,
                    quantityBehavior,
                    notes,
                    section.ProductRequirements.Count);
                dbContext.ActivityProductRequirements.Add(requirement);
                return requirement.Id;
            },
            "The product could not be assigned because it is no longer active or is already in the section.",
            cancellationToken);

    public Task UpdateProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        decimal quantity,
        bool isRequired,
        ProductQuantityBehavior quantityBehavior,
        string? notes,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadActivityAsync(activityId, cancellationToken);
                EnsureEditable(activity);
                var requirement = await LoadProductRequirementAsync(
                    activityId,
                    sectionId,
                    requirementId,
                    cancellationToken);
                requirement.UpdateDetails(quantity, isRequired, quantityBehavior, notes);
            },
            "The product requirement could not be updated because the activity changed.",
            cancellationToken);

    public Task MoveProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        int targetIndex,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadActivityAsync(activityId, cancellationToken);
                EnsureEditable(activity);
                await LoadSectionAsync(activityId, sectionId, cancellationToken);
                var requirements = await dbContext.ActivityProductRequirements
                    .Where(requirement => requirement.SectionId == sectionId)
                    .OrderBy(requirement => requirement.SortOrder)
                    .ThenBy(requirement => requirement.Id)
                    .ToListAsync(cancellationToken);
                var requirement = requirements.SingleOrDefault(
                    current => current.Id == requirementId);
                Guard.Against.NotFound(requirementId, requirement);

                requirements.Remove(requirement);
                EnsureTargetIndex(targetIndex, requirements.Count);
                requirements.Insert(targetIndex, requirement);
                RenumberRequirements(requirements);
            },
            "The product requirement could not be moved because the activity changed.",
            cancellationToken);

    public Task DeleteProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadActivityAsync(activityId, cancellationToken);
                EnsureEditable(activity);
                var section = await dbContext.ActivityRequirementSections
                    .Include(current => current.ProductRequirements)
                    .SingleOrDefaultAsync(
                        current => current.Id == sectionId
                            && current.ActivityId == activityId,
                        cancellationToken);
                Guard.Against.NotFound(sectionId, section);
                var requirement = section.ProductRequirements.SingleOrDefault(
                    current => current.Id == requirementId);
                Guard.Against.NotFound(requirementId, requirement);

                section.RemoveProductRequirement(requirement);
                RenumberRequirements(
                    section.ProductRequirements
                        .OrderBy(current => current.SortOrder)
                        .ThenBy(current => current.Id)
                        .ToList());
                dbContext.ActivityProductRequirements.Remove(requirement);
            },
            "The product requirement could not be removed because the activity changed.",
            cancellationToken);

    private async Task<Activity> LoadActivityAsync(
        Guid activityId,
        CancellationToken cancellationToken)
    {
        var activity = await dbContext.ActivityCatalogNodes
            .OfType<Activity>()
            .SingleOrDefaultAsync(current => current.Id == activityId, cancellationToken);
        Guard.Against.NotFound(activityId, activity);
        return activity;
    }

    private async Task<ActivityRequirementSection> LoadSectionAsync(
        Guid activityId,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        var section = await dbContext.ActivityRequirementSections.SingleOrDefaultAsync(
            current => current.Id == sectionId && current.ActivityId == activityId,
            cancellationToken);
        Guard.Against.NotFound(sectionId, section);
        return section;
    }

    private async Task<ActivityProductRequirement> LoadProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        CancellationToken cancellationToken)
    {
        var requirement = await dbContext.ActivityProductRequirements
            .SingleOrDefaultAsync(
                current => current.Id == requirementId
                    && current.SectionId == sectionId
                    && current.Section.ActivityId == activityId,
                cancellationToken);
        Guard.Against.NotFound(requirementId, requirement);
        return requirement;
    }

    private static void EnsureEditable(Activity activity)
    {
        if (activity.Status == ActivityStatus.Archived)
        {
            throw new ActivityCatalogConflictException(
                "Requirements cannot be changed while the activity is archived.");
        }

        activity.EnsureRequirementsEditable();
    }

    private static void EnsureTargetIndex(int targetIndex, int destinationCount)
    {
        if (targetIndex < 0 || targetIndex > destinationCount)
        {
            throw new ActivityCatalogConflictException(
                "The requested position is no longer available. Refresh the activity and try again.");
        }
    }

    private static void RenumberSections(IReadOnlyList<ActivityRequirementSection> sections)
    {
        for (var index = 0; index < sections.Count; index++)
        {
            sections[index].SetSortOrder(index);
        }
    }

    private static void RenumberRequirements(
        IReadOnlyList<ActivityProductRequirement> requirements)
    {
        for (var index = 0; index < requirements.Count; index++)
        {
            requirements[index].SetSortOrder(index);
        }
    }

    private async Task ExecuteMutationAsync(
        Func<Task> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken)
    {
        await ExecuteMutationAsync(
            async () =>
            {
                await mutation();
                return true;
            },
            persistenceConflictMessage,
            cancellationToken);
    }

    private async Task<TResult> ExecuteMutationAsync<TResult>(
        Func<Task<TResult>> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var result = await mutation();
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new ActivityCatalogConflictException(
                persistenceConflictMessage,
                exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
