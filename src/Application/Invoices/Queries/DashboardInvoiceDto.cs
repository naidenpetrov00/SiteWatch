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
    public string TaxIdentifier { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string ContactPerson { get; init; } = string.Empty;
    public DateTimeOffset PaymentTerm { get; init; }
    public decimal TotalValueExcludingVat { get; init; }
    public decimal Vat { get; init; }
    public decimal TotalValueIncludingVat { get; init; }
    public DateTimeOffset? PaymentDate { get; init; }
    public DateTimeOffset? PaymentTime { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public IReadOnlyList<DashboardInvoiceSiteAllocationDto> SiteAllocations { get; init; } = [];

    public sealed class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Invoice, DashboardInvoiceDto>()
                .ForMember(
                    destination => destination.SiteAllocations,
                    options => options.MapFrom(source => source.SitePayments))
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

            CreateMap<SitePayment, DashboardInvoiceSiteAllocationDto>()
                .ForMember(
                    destination => destination.SiteNumberId,
                    options => options.MapFrom(source => source.Site.NumberId))
                .ForMember(
                    destination => destination.SiteName,
                    options => options.MapFrom(source => source.Site.Name.Value))
                .ForMember(
                    destination => destination.Direction,
                    options => options.MapFrom(source => source.Direction.ToString()));
        }
    }
}

public sealed record DashboardInvoiceSiteAllocationDto
{
    public Guid SiteId { get; init; }
    public int SiteNumberId { get; init; }
    public string SiteName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Direction { get; init; } = string.Empty;
}
