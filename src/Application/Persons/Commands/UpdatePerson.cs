using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Persons.Commands;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdatePersonCommand : IRequest
{
    public Guid Id { get; set; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public List<PersonAddressDto>? Addresses { get; init; }
    public List<PersonContactDto>? Contacts { get; init; }
    public List<PersonBankAccountDto>? BankAccounts { get; init; }
}

public sealed class UpdatePersonHandler(IPersonService personService)
    : IRequestHandler<UpdatePersonCommand>
{
    public Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken) =>
        personService.UpdateAsync(request, cancellationToken);
}
