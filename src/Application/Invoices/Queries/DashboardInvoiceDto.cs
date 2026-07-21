using AutoMapper;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Invoices.Queries;

public sealed record DashboardInvoiceDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public Guid SupplierId { get; init; }
    public string SupplierDisplayLabel { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTimeOffset Date { get; init; }
    public string Eik { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string ContactPerson { get; init; } = string.Empty;
    public string Iban { get; init; } = string.Empty;
    public DateTimeOffset PaymentTerm { get; init; }
    public decimal TotalValueExcludingVat { get; init; }
    public decimal Vat { get; init; }
    public decimal TotalValueIncludingVat { get; init; }
    public DateTimeOffset? PaymentDate { get; init; }
    public DateTimeOffset? PaymentTime { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;

    public sealed class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Invoice, DashboardInvoiceDto>()
                .ForMember(
                    d => d.SupplierDisplayLabel,
                    o =>
                        o.MapFrom(s =>
                            s.Supplier.Type == PersonType.Company
                                ? s.Supplier.CompanyName ?? string.Empty
                                : s.Supplier.MiddleName == null
                                    ? (s.Supplier.FirstName ?? string.Empty)
                                        + " "
                                        + (s.Supplier.LastName ?? string.Empty)
                                    : (s.Supplier.FirstName ?? string.Empty)
                                        + " "
                                        + (s.Supplier.MiddleName ?? string.Empty)
                                        + " "
                                        + (s.Supplier.LastName ?? string.Empty)
                        )
                );
        }
    }
}
