using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.TerritorialStructure;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record TerritorialEntriesQuery : IRequest<IReadOnlyList<TerritorialEntryDto>>
{
    public TerritorialEntryKind Kind { get; init; }
    public Guid? ParentId { get; init; }
    public string? SearchTerm { get; init; }
    public bool IncludeInactive { get; init; }
}

public sealed class TerritorialStructureQueryHandler(ITerritorialStructureService service)
    : IRequestHandler<TerritorialEntriesQuery, IReadOnlyList<TerritorialEntryDto>>
{
    public Task<IReadOnlyList<TerritorialEntryDto>> Handle(TerritorialEntriesQuery request, CancellationToken cancellationToken) =>
        service.GetEntriesAsync(request, cancellationToken);
}

public sealed class TerritorialEntriesQueryValidator : AbstractValidator<TerritorialEntriesQuery>
{
    public TerritorialEntriesQueryValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.SearchTerm).MaximumLength(100);
        RuleFor(x => x.ParentId).NotEmpty().When(x => x.Kind != TerritorialEntryKind.Municipality);
    }
}
