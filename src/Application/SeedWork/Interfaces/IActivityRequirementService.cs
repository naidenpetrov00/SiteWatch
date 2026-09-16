using Domain.SeedWork.Enums;

namespace Application.SeedWork.Interfaces;

public interface IActivityRequirementService
{
    Task<Guid> CreateSectionAsync(
        Guid activityId,
        string? name,
        decimal basisQuantity,
        ActivityMeasurementUnit measurementUnit,
        CancellationToken cancellationToken);

    Task UpdateSectionAsync(
        Guid activityId,
        Guid sectionId,
        string? name,
        decimal basisQuantity,
        ActivityMeasurementUnit measurementUnit,
        CancellationToken cancellationToken);

    Task MoveSectionAsync(
        Guid activityId,
        Guid sectionId,
        int targetIndex,
        CancellationToken cancellationToken);

    Task DeleteSectionAsync(
        Guid activityId,
        Guid sectionId,
        CancellationToken cancellationToken);

    Task<Guid> CreateProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid productId,
        decimal quantity,
        bool isRequired,
        ProductQuantityBehavior quantityBehavior,
        string? notes,
        CancellationToken cancellationToken);

    Task UpdateProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        decimal quantity,
        bool isRequired,
        ProductQuantityBehavior quantityBehavior,
        string? notes,
        CancellationToken cancellationToken);

    Task MoveProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        int targetIndex,
        CancellationToken cancellationToken);

    Task DeleteProductRequirementAsync(
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        CancellationToken cancellationToken);
}
