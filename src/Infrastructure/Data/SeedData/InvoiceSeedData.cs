using Domain.Entities;

namespace Infrastructure.Data.SeedData;

public static class InvoiceSeedData
{
    public static List<Invoice> Create(IReadOnlyList<Person> persons)
    {
        var now = DateTimeOffset.UtcNow;
        var invoices = new List<Invoice>
        {
            Invoice.Create(
                persons[0].Id,
                persons[0],
                "INV-2026-0001",
                now.AddDays(-10),
                "8001011234",
                "Vitosha 17, Sofia",
                "ivan.petrov@example.com",
                "+359888100001",
                "Ivan Petrov",
                now.AddDays(20),
                1000m,
                200m,
                1200m,
                "Bank transfer",
                now.AddDays(20)),
            Invoice.Create(
                persons[1].Id,
                persons[1],
                "INV-2026-0002",
                now.AddDays(-5),
                "8502022345",
                "Dondukov 11, Sofia",
                "maria.georgieva@example.com",
                "+359888100002",
                "Maria Georgieva",
                now.AddDays(10),
                750m,
                150m,
                900m,
                "Card",
                now.AddDays(10)),
            Invoice.Create(
                persons[2].Id,
                persons[2],
                "INV-2026-0003",
                now.AddDays(-2),
                "123456789",
                "Kestenova Gora 24, Sofia",
                "billing@sitewatch.example",
                "+359888100003",
                "SiteWatch Services",
                now.AddDays(43),
                2500m,
                500m,
                3000m,
                "Bank transfer",
                now.AddDays(43)),
        };

        foreach (var invoice in invoices)
        {
            invoice.Created = now;
            invoice.CreatedBy = "System";
            invoice.LastModified = now;
            invoice.LastModifiedBy = "System";
        }

        return invoices;
    }
}
