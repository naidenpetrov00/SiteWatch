using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SeedWork.Extension;

internal static class ModelBuilderExtension
{
    internal static void AddSequences(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>("InvoiceNumberIds", "dbo");
        modelBuilder.HasSequence<int>("PersonNumberIds", "dbo")
            .StartsAt(1)
            .IncrementsBy(1);
        modelBuilder.HasSequence<int>("ApplicationUserNumberIds", "dbo")
            .StartsAt(1)
            .IncrementsBy(1);
    }
}
