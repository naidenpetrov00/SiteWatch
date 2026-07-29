using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Identity.Commands.SetAdministratorClaim;

[Authorize(Roles = UserRoles.Administrator)]
public class SetAdministratorClaimCommand : IRequest<IdentityResultModel>
{
    public required string UserId { get; set; }
}

public class SetAdministratorClaimHandler
    : IRequestHandler<SetAdministratorClaimCommand, IdentityResultModel>
{
    private readonly IIdentityService identityService;

    public SetAdministratorClaimHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<IdentityResultModel> Handle(
        SetAdministratorClaimCommand request,
        CancellationToken cancellationToken
    ) => await identityService.AssignRoleAsync(request.UserId, UserRoles.Administrator);
}
