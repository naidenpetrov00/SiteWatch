using Api.Services;
using Microsoft.AspNetCore.DataProtection;

namespace Api.Tests.Services;

public sealed class InvoiceFileAccessTicketServiceTests
{
    [Fact]
    public void Create_then_read_returns_the_original_access_scope()
    {
        var service = CreateService();
        var siteId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        var ticket = service.Create(siteId, invoiceId, "worker-1", DateTimeOffset.UtcNow.AddMinutes(5));

        Assert.True(service.TryRead(ticket, out var access));
        Assert.Equal(siteId, access!.SiteId);
        Assert.Equal(invoiceId, access.InvoiceId);
        Assert.Equal("worker-1", access.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-ticket")]
    public void TryRead_rejects_blank_or_malformed_tickets(string ticket)
    {
        var service = CreateService();

        Assert.False(service.TryRead(ticket, out var access));
        Assert.Null(access);
    }

    [Fact]
    public void TryRead_rejects_an_expired_ticket()
    {
        var service = CreateService();
        var ticket = service.Create(Guid.NewGuid(), Guid.NewGuid(), "worker-1", DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.False(service.TryRead(ticket, out var access));
        Assert.Null(access);
    }

    [Fact]
    public void TryRead_rejects_a_ticket_created_by_a_different_protector()
    {
        var ticket = CreateService().Create(Guid.NewGuid(), Guid.NewGuid(), "worker-1", DateTimeOffset.UtcNow.AddMinutes(5));

        Assert.False(CreateService().TryRead(ticket, out var access));
        Assert.Null(access);
    }

    private static InvoiceFileAccessTicketService CreateService() =>
        new(new EphemeralDataProtectionProvider());
}
