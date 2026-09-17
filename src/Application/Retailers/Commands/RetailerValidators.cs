using Application.SeedWork.Interfaces;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Retailers.Commands;

public abstract class RetailerUpsertValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : RetailerUpsertDto
{
    protected RetailerUpsertValidator(IApplicationDbContext dbContext)
    {
        RuleFor(request => request.DisplayName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(Retailer.IsMeaningfulDisplayName)
            .WithMessage(
                $"DisplayName must contain at least one letter or digit and be at most {Retailer.MaxDisplayNameLength} characters after whitespace normalization.");
        RuleFor(request => request.BaseWebsiteUrl)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(value => RetailerWebsite.TryCreate(value, out _))
            .WithMessage(
                "BaseWebsiteUrl must be an absolute HTTP or HTTPS origin without credentials, a path, query, or fragment.");
        RuleFor(request => request.Notes)
            .Must(notes => string.IsNullOrWhiteSpace(notes)
                || notes.Trim().Length <= Retailer.MaxNotesLength)
            .WithMessage(
                $"Notes must be at most {Retailer.MaxNotesLength} characters after trimming.");
        RuleFor(request => request.CompanyPersonId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (companyPersonId, cancellationToken) =>
                await dbContext.Persons
                    .AsNoTracking()
                    .AnyAsync(
                        person => person.Id == companyPersonId
                            && person.Type == PersonType.Company,
                        cancellationToken))
            .WithMessage("CompanyPersonId must reference an existing company Person.");
    }
}

public sealed class CreateRetailerValidator
    : RetailerUpsertValidator<CreateRetailerCommand>
{
    public CreateRetailerValidator(IApplicationDbContext dbContext) : base(dbContext)
    {
    }
}

public sealed class UpdateRetailerValidator
    : RetailerUpsertValidator<UpdateRetailerCommand>
{
    public UpdateRetailerValidator(IApplicationDbContext dbContext) : base(dbContext)
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}

public sealed class SetRetailerActiveValidator
    : AbstractValidator<SetRetailerActiveCommand>
{
    public SetRetailerActiveValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
