using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.TerritorialStructure;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateTerritorialEntryCommand(
    TerritorialEntryKind Kind,
    Guid? ParentId,
    string Name) : IRequest<Guid>;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateTerritorialEntryCommand : IRequest
{
    public Guid Id { get; set; }
    public TerritorialEntryKind Kind { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteTerritorialEntryCommand(TerritorialEntryKind Kind, Guid Id) : IRequest;

public sealed class TerritorialStructureCommandHandler(ITerritorialStructureService service)
    : IRequestHandler<CreateTerritorialEntryCommand, Guid>, IRequestHandler<UpdateTerritorialEntryCommand>,
      IRequestHandler<DeleteTerritorialEntryCommand>
{
    public Task<Guid> Handle(CreateTerritorialEntryCommand request, CancellationToken cancellationToken) =>
        service.CreateAsync(request.Kind, request.ParentId, request.Name, cancellationToken);

    public Task Handle(UpdateTerritorialEntryCommand request, CancellationToken cancellationToken) =>
        service.UpdateAsync(request.Kind, request.Id, request.Name, request.IsActive, cancellationToken);

    public Task Handle(DeleteTerritorialEntryCommand request, CancellationToken cancellationToken) =>
        service.DeleteAsync(request.Kind, request.Id, cancellationToken);
}

public sealed class DeleteTerritorialEntryValidator : AbstractValidator<DeleteTerritorialEntryCommand>
{
    public DeleteTerritorialEntryValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class CreateTerritorialEntryValidator : AbstractValidator<CreateTerritorialEntryCommand>
{
    public CreateTerritorialEntryValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ParentId).NotEmpty().When(x => x.Kind != TerritorialEntryKind.Municipality);
        RuleFor(x => x.ParentId).Null().When(x => x.Kind == TerritorialEntryKind.Municipality);
    }
}

public sealed class UpdateTerritorialEntryValidator : AbstractValidator<UpdateTerritorialEntryCommand>
{
    public UpdateTerritorialEntryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
