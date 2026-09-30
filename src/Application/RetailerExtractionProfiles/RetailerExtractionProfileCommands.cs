using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.RetailerExtractionProfiles;

/// <summary>
/// Creates the single editable draft for a retailer by cloning a supplied or active published
/// profile; only the first profile is initialized as a blank draft.
/// </summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateRetailerExtractionDraftCommand
    : IRequest<RetailerExtractionProfileDetailsDto>
{
    public Guid RetailerId { get; set; }
    public Guid? SourcePublishedProfileId { get; init; }
}

/// <summary>Deletes an editable draft and all of its configuration.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteRetailerExtractionDraftCommand(Guid RetailerId, Guid ProfileId)
    : IRequest;

/// <summary>Replaces the exact HTTPS host allowlist for a draft.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateRetailerExtractionAllowedHostsCommand
    : IRequest<RetailerExtractionProfileDetailsDto>
{
    public Guid RetailerId { get; set; }
    public Guid ProfileId { get; set; }
    public IReadOnlyList<string> AllowedHosts { get; init; } = [];
}

/// <summary>Adds an extraction rule at the end of a draft.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record AddRetailerExtractionRuleCommand
    : RetailerExtractionRuleRequest, IRequest<RetailerExtractionProfileDetailsDto>
{
    public Guid RetailerId { get; set; }
    public Guid ProfileId { get; set; }
}

/// <summary>Updates one extraction rule in a draft without changing its priority.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateRetailerExtractionRuleCommand
    : RetailerExtractionRuleRequest, IRequest<RetailerExtractionProfileDetailsDto>
{
    public Guid RetailerId { get; set; }
    public Guid ProfileId { get; set; }
    public Guid RuleId { get; set; }
}

/// <summary>Deletes one extraction rule from a draft.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteRetailerExtractionRuleCommand(
    Guid RetailerId,
    Guid ProfileId,
    Guid RuleId) : IRequest;

/// <summary>Enables or disables one extraction rule in a draft.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record SetRetailerExtractionRuleEnabledCommand(
    Guid RetailerId,
    Guid ProfileId,
    Guid RuleId,
    bool IsEnabled) : IRequest<RetailerExtractionProfileDetailsDto>;

/// <summary>Assigns the complete ordered rule list for a draft.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ReorderRetailerExtractionRulesCommand
    : IRequest<RetailerExtractionProfileDetailsDto>
{
    public Guid RetailerId { get; set; }
    public Guid ProfileId { get; set; }
    public IReadOnlyList<Guid> OrderedRuleIds { get; init; } = [];
}

/// <summary>Publishes and activates a complete draft.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record PublishRetailerExtractionProfileCommand(Guid RetailerId, Guid ProfileId)
    : IRequest<RetailerExtractionProfileDetailsDto>;

/// <summary>Activates a retained published profile version.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ActivateRetailerExtractionProfileCommand(Guid RetailerId, Guid ProfileId)
    : IRequest<RetailerExtractionProfileDetailsDto>;

public sealed class CreateRetailerExtractionDraftHandler(IRetailerExtractionProfileService service)
    : IRequestHandler<CreateRetailerExtractionDraftCommand, RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        CreateRetailerExtractionDraftCommand request,
        CancellationToken cancellationToken) =>
        service.CreateDraftAsync(
            request.RetailerId,
            request.SourcePublishedProfileId,
            cancellationToken);
}

public sealed class DeleteRetailerExtractionDraftHandler(IRetailerExtractionProfileService service)
    : IRequestHandler<DeleteRetailerExtractionDraftCommand>
{
    public Task Handle(
        DeleteRetailerExtractionDraftCommand request,
        CancellationToken cancellationToken) =>
        service.DeleteDraftAsync(request.RetailerId, request.ProfileId, cancellationToken);
}

public sealed class UpdateRetailerExtractionAllowedHostsHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<
        UpdateRetailerExtractionAllowedHostsCommand,
        RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        UpdateRetailerExtractionAllowedHostsCommand request,
        CancellationToken cancellationToken) =>
        service.UpdateAllowedHostsAsync(
            request.RetailerId,
            request.ProfileId,
            request.AllowedHosts,
            cancellationToken);
}

public sealed class AddRetailerExtractionRuleHandler(IRetailerExtractionProfileService service)
    : IRequestHandler<AddRetailerExtractionRuleCommand, RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        AddRetailerExtractionRuleCommand request,
        CancellationToken cancellationToken) =>
        service.AddRuleAsync(request.RetailerId, request.ProfileId, request, cancellationToken);
}

public sealed class UpdateRetailerExtractionRuleHandler(IRetailerExtractionProfileService service)
    : IRequestHandler<UpdateRetailerExtractionRuleCommand, RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        UpdateRetailerExtractionRuleCommand request,
        CancellationToken cancellationToken) =>
        service.UpdateRuleAsync(
            request.RetailerId,
            request.ProfileId,
            request.RuleId,
            request,
            cancellationToken);
}

public sealed class DeleteRetailerExtractionRuleHandler(IRetailerExtractionProfileService service)
    : IRequestHandler<DeleteRetailerExtractionRuleCommand>
{
    public Task Handle(
        DeleteRetailerExtractionRuleCommand request,
        CancellationToken cancellationToken) =>
        service.DeleteRuleAsync(
            request.RetailerId,
            request.ProfileId,
            request.RuleId,
            cancellationToken);
}

public sealed class SetRetailerExtractionRuleEnabledHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<
        SetRetailerExtractionRuleEnabledCommand,
        RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        SetRetailerExtractionRuleEnabledCommand request,
        CancellationToken cancellationToken) =>
        service.SetRuleEnabledAsync(
            request.RetailerId,
            request.ProfileId,
            request.RuleId,
            request.IsEnabled,
            cancellationToken);
}

public sealed class ReorderRetailerExtractionRulesHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<
        ReorderRetailerExtractionRulesCommand,
        RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        ReorderRetailerExtractionRulesCommand request,
        CancellationToken cancellationToken) =>
        service.ReorderRulesAsync(
            request.RetailerId,
            request.ProfileId,
            request.OrderedRuleIds,
            cancellationToken);
}

public sealed class PublishRetailerExtractionProfileHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<
        PublishRetailerExtractionProfileCommand,
        RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        PublishRetailerExtractionProfileCommand request,
        CancellationToken cancellationToken) =>
        service.PublishAsync(request.RetailerId, request.ProfileId, cancellationToken);
}

public sealed class ActivateRetailerExtractionProfileHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<
        ActivateRetailerExtractionProfileCommand,
        RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        ActivateRetailerExtractionProfileCommand request,
        CancellationToken cancellationToken) =>
        service.ActivateAsync(request.RetailerId, request.ProfileId, cancellationToken);
}
