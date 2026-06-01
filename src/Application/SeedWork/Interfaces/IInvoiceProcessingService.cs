namespace Application.SeedWork.Interfaces;

public interface IInvoiceProcessingService
{
    Task ProcessAsync(Guid siteId, Guid invoiceId, CancellationToken cancellationToken = default);
}
