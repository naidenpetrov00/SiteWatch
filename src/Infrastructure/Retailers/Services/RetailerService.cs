using System.Data;
using Application.Retailers;
using Application.Retailers.Commands;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Retailers.Services;

public sealed class RetailerService(ApplicationDbContext dbContext) : IRetailerService
{
    public Task<Guid> CreateAsync(
        RetailerUpsertDto request,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var companyPerson = await LoadCompanyPersonAsync(
                    request.CompanyPersonId,
                    cancellationToken);
                await EnsureUniqueIdentityAsync(
                    request.DisplayName,
                    null,
                    cancellationToken);

                var retailer = Retailer.Create(
                    request.DisplayName,
                    companyPerson,
                    request.BaseWebsiteUrl,
                    request.Notes);
                dbContext.Retailers.Add(retailer);
                return retailer.Id;
            },
            "A retailer with this display name already exists.",
            cancellationToken);

    public Task UpdateAsync(
        UpdateRetailerCommand request,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var retailer = await dbContext.Retailers
                    .SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
                Guard.Against.NotFound(request.Id, retailer);

                var companyPerson = await LoadCompanyPersonAsync(
                    request.CompanyPersonId,
                    cancellationToken);
                await EnsureCompanyRetainsRetailerAsync(
                    retailer,
                    request.CompanyPersonId,
                    cancellationToken);
                await EnsureUniqueIdentityAsync(
                    request.DisplayName,
                    request.Id,
                    cancellationToken);

                retailer.UpdateDetails(
                    request.DisplayName,
                    companyPerson,
                    request.BaseWebsiteUrl,
                    request.Notes);
            },
            "A retailer with this display name already exists.",
            cancellationToken);

    public Task SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var retailer = await dbContext.Retailers
                    .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
                Guard.Against.NotFound(id, retailer);

                if (isActive)
                {
                    retailer.Activate();
                }
                else
                {
                    retailer.Deactivate();
                }
            },
            "The retailer status could not be changed.",
            cancellationToken);

    private async Task<Person> LoadCompanyPersonAsync(
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        var companyPerson = await dbContext.Persons
            .SingleOrDefaultAsync(
                person => person.Id == companyPersonId
                    && person.Type == PersonType.Company,
                cancellationToken);
        if (companyPerson is null)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(RetailerUpsertDto.CompanyPersonId),
                    "CompanyPersonId must reference an existing company Person.")
            ]);
        }

        return companyPerson;
    }

    private async Task EnsureUniqueIdentityAsync(
        string displayName,
        Guid? excludedRetailerId,
        CancellationToken cancellationToken)
    {
        var normalizedName = Retailer.NormalizeNameKey(displayName);
        var conflictingRetailer = await dbContext.Retailers
            .AsNoTracking()
            .Where(retailer => !excludedRetailerId.HasValue
                || retailer.Id != excludedRetailerId.Value)
            .AnyAsync(retailer => retailer.NormalizedName == normalizedName, cancellationToken);

        if (conflictingRetailer)
        {
            throw new RetailerConflictException(
                "A retailer with this normalized display name already exists.");
        }
    }

    private async Task EnsureCompanyRetainsRetailerAsync(
        Retailer retailer,
        Guid requestedCompanyPersonId,
        CancellationToken cancellationToken)
    {
        if (retailer.CompanyPersonId == requestedCompanyPersonId)
        {
            return;
        }

        var companyOwnsProfiles = await dbContext.RetailerExtractionProfiles.AnyAsync(
            profile => profile.CompanyPersonId == retailer.CompanyPersonId,
            cancellationToken);
        if (!companyOwnsProfiles)
        {
            return;
        }

        var hasAnotherRetailer = await dbContext.Retailers.AnyAsync(
            item => item.CompanyPersonId == retailer.CompanyPersonId
                && item.Id != retailer.Id,
            cancellationToken);
        if (!hasAnotherRetailer)
        {
            throw new RetailerConflictException(
                "This retailer is the last location associated with a company Person that owns extraction profiles.");
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
            throw new RetailerConflictException(persistenceConflictMessage, exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
