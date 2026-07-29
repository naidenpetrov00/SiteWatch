using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Identity.Commands.SetUserRole;

[Authorize(Roles = UserRoles.Administrator)]
public sealed class SetUserRoleCommand : IRequest<IdentityResultModel>
{
    public required string UserId { get; set; }
    public required string Role { get; set; }
}

public sealed class SetUserRoleCommandValidator : AbstractValidator<SetUserRoleCommand>
{
    public SetUserRoleCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Role)
            .NotEmpty()
            .Must(UserRoles.IsSupported)
            .WithMessage("Role must be Administrator, Client, or Worker.");
    }
}

public sealed class SetUserRoleHandler(IIdentityService identityService)
    : IRequestHandler<SetUserRoleCommand, IdentityResultModel>
{
    public Task<IdentityResultModel> Handle(
        SetUserRoleCommand request,
        CancellationToken cancellationToken
    ) => identityService.AssignRoleAsync(request.UserId, request.Role);
}
