using System.Data;
using Application.Retailers;
using Application.Retailers.Commands;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
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
                    request.BaseWebsiteUrl,
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
            "A retailer with this display name or website host already exists.",
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
                await EnsureUniqueIdentityAsync(
                    request.DisplayName,
                    request.BaseWebsiteUrl,
                    request.Id,
                    cancellationToken);

                retailer.UpdateDetails(
                    request.DisplayName,
                    companyPerson,
                    request.BaseWebsiteUrl,
                    request.Notes);
            },
            "A retailer with this display name or website host already exists.",
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
        string baseWebsiteUrl,
        Guid? excludedRetailerId,
        CancellationToken cancellationToken)
    {
        var normalizedName = Retailer.NormalizeNameKey(displayName);
        var normalizedHost = RetailerWebsite.Create(baseWebsiteUrl).NormalizedHost;
        var conflictingRetailer = await dbContext.Retailers
            .AsNoTracking()
            .Where(retailer => !excludedRetailerId.HasValue
                || retailer.Id != excludedRetailerId.Value)
            .Where(retailer => retailer.NormalizedName == normalizedName
                || retailer.NormalizedWebsiteHost == normalizedHost)
            .Select(retailer => new
            {
                SameName = retailer.NormalizedName == normalizedName,
                SameHost = retailer.NormalizedWebsiteHost == normalizedHost
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (conflictingRetailer?.SameName == true)
        {
            throw new RetailerConflictException(
                "A retailer with this normalized display name already exists.");
        }

        if (conflictingRetailer?.SameHost == true)
        {
            throw new RetailerConflictException(
                "A retailer already represents this website host.");
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
