using AutoMapper;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Invoices.Queries;

public sealed record DashboardInvoiceDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public bool IsComplete { get; init; }
    public Guid? SupplierId { get; init; }
    public string? SupplierDisplayLabel { get; init; }
    public string? SubmittedFromSiteName { get; init; }
    public string? InvoiceNumber { get; init; }
    public DateTimeOffset? Date { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? TaxIdentifier { get; init; }
    public string? Address { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? ContactPerson { get; init; }
    public DateTimeOffset? PaymentTerm { get; init; }
    public decimal? TotalValueExcludingVat { get; init; }
    public decimal? VatRate { get; init; }
    public decimal? Vat { get; init; }
    public decimal? TotalValueIncludingVat { get; init; }
    public DateTimeOffset? PaymentDate { get; init; }
    public DateTimeOffset? PaymentTime { get; init; }
    public string? PaymentMethod { get; init; }
    public IReadOnlyList<DashboardInvoiceSiteAllocationDto> SiteAllocations { get; init; } = [];

    public sealed class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Invoice, DashboardInvoiceDto>()
                .ForMember(
                    destination => destination.IsComplete,
                    options => options.MapFrom(source => source.Status == InvoiceStatus.Complete))
                .ForMember(
                    destination => destination.VatRate,
                    options => options.MapFrom(source =>
                        source.VatRate
                        ?? (source.TotalValueExcludingVat.HasValue
                            && source.TotalValueExcludingVat.Value > 0m
                            && source.Vat.HasValue
                                ? source.Vat.Value * 100m / source.TotalValueExcludingVat.Value
                                : (decimal?)null)))
                .ForMember(
                    destination => destination.SiteAllocations,
                    options => options.MapFrom(source => source.SitePayments))
                .ForMember(
                    d => d.SupplierDisplayLabel,
                    o =>
                        o.MapFrom(s => s.Supplier == null
                            ? null
                            : s.Supplier.Type == PersonType.Company
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
                )
                .ForMember(
                    destination => destination.SubmittedFromSiteName,
                    options => options.MapFrom(source =>
                        source.SubmittedFromSite == null
                            ? null
                            : source.SubmittedFromSite.Name.Value));

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
