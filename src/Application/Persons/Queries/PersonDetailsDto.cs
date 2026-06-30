using Application.Persons.Commands;
using AutoMapper;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Persons.Queries;

public sealed record PersonDetailsDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? LegalForm { get; init; }
    public string? Egn { get; init; }
    public string? Eik { get; init; }
    public string VatNumber { get; init; } = string.Empty;
    public IReadOnlyList<PersonAddressDto> Addresses { get; init; } = [];
    public IReadOnlyList<PersonContactDto> Contacts { get; init; } = [];
    public IReadOnlyList<PersonBankAccountDto> BankAccounts { get; init; } = [];

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Person, PersonDetailsDto>()
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
                .ForMember(
                    d => d.DisplayName,
                    o => o.MapFrom(s =>
                        s.Type == PersonType.Company
                            ? s.CompanyName ?? string.Empty
                            : s.MiddleName == null
                                ? (s.FirstName ?? string.Empty) + " " + (s.LastName ?? string.Empty)
                                : (s.FirstName ?? string.Empty)
                                    + " "
                                    + (s.MiddleName ?? string.Empty)
                                    + " "
                                    + (s.LastName ?? string.Empty))
                )
                .ForMember(d => d.LegalForm, o => o.MapFrom(s => s.LegalForm == null ? null : s.LegalForm.ToString()));

            CreateMap<PersonAddress, PersonAddressDto>();
            CreateMap<PersonContact, PersonContactDto>();
            CreateMap<PersonBankAccount, PersonBankAccountDto>();
        }
    }
}
