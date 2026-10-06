using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

internal sealed class RetailerPriceCollectionWorkerLeaseConfiguration
    : IEntityTypeConfiguration<RetailerPriceCollectionWorkerLease>
{
    public void Configure(EntityTypeBuilder<RetailerPriceCollectionWorkerLease> builder)
    {
        builder.ToTable("RetailerPriceCollectionWorkerLeases");
        builder.HasKey(lease => lease.Id);
        builder.Property(lease => lease.Id).HasMaxLength(100);
        builder.Property(lease => lease.RowVersion).IsRowVersion();
    }
}
