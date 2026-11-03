using System.Data;
using Application.Offers.Defaults;
using Application.Retailers;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Offers.Services;

public sealed class DefaultOfferRetailerService(ApplicationDbContext dbContext)
    : IDefaultOfferRetailerService
{
    public async Task<IReadOnlyList<DefaultOfferRetailerDto>> GetAsync(
        CancellationToken cancellationToken)
    {
        var defaults = await dbContext.DefaultOfferRetailers
            .AsNoTracking()
            .Include(item => item.Retailer)
            .Where(item => item.Retailer.IsActive)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

        return defaults
            .Select(item => DefaultOfferRetailerDto.From(item.Retailer))
            .ToList();
    }

    public async Task<IReadOnlyList<DefaultOfferRetailerDto>> ReplaceAsync(
        IReadOnlyList<Guid> retailerIds,
        CancellationToken cancellationToken)
    {
        var requestedIds = retailerIds.ToList();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var existingDefaults = await dbContext.DefaultOfferRetailers
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .ToListAsync(cancellationToken);
            List<Retailer> retailers = requestedIds.Count == 0
                ? []
                : await dbContext.Retailers
                    .Where(retailer => requestedIds.Contains(retailer.Id))
                    .ToListAsync(cancellationToken);
            var retailersById = retailers.ToDictionary(retailer => retailer.Id);

            if (requestedIds.Any(id =>
                    !retailersById.TryGetValue(id, out var retailer)
                    || !retailer.IsActive))
            {
                throw new ValidationException(
                [
                    new ValidationFailure(
                        nameof(ReplaceDefaultOfferRetailersCommand.RetailerIds),
                        "RetailerIds must reference existing active retailers.")
                ]);
            }

            if (existingDefaults.Count > 0)
            {
                dbContext.DefaultOfferRetailers.RemoveRange(existingDefaults);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var replacements = requestedIds
                .Select((id, sortOrder) =>
                    DefaultOfferRetailer.Create(retailersById[id], sortOrder))
                .ToList();
            dbContext.DefaultOfferRetailers.AddRange(replacements);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return requestedIds
                .Select(id => DefaultOfferRetailerDto.From(retailersById[id]))
                .ToList();
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RetailerConflictException(
                "The default Offer retailers could not be replaced.",
                exception);
        }
        catch (SqlException exception) when (exception.Number is 1205 or 1222)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RetailerConflictException(
                "The default Offer retailers could not be replaced.",
                exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
