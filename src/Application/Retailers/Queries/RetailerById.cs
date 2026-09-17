using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Retailers.Queries;

/// <summary>Loads a retailer and its legal-company summary.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerByIdQuery(Guid RetailerId) : IRequest<RetailerDetailsDto>;

public sealed class RetailerByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RetailerByIdQuery, RetailerDetailsDto>
{
    public async Task<RetailerDetailsDto> Handle(
        RetailerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var retailer = await dbContext.Retailers
            .AsNoTracking()
            .Include(item => item.CompanyPerson)
            .SingleOrDefaultAsync(item => item.Id == request.RetailerId, cancellationToken);

        Guard.Against.NotFound(request.RetailerId, retailer);
        return RetailerDetailsDto.From(retailer);
    }
}
