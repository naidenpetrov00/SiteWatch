using Application.Retailers.Commands;

namespace Application.SeedWork.Interfaces;

public interface IRetailerService
{
    Task<Guid> CreateAsync(RetailerUpsertDto request, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateRetailerCommand request, CancellationToken cancellationToken);
    Task SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
}
