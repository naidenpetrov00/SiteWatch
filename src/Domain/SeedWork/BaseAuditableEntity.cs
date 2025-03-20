namespace Domain.SeedWork;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset Created { get; private set; }

    public string? CreatedBy { get; private set; }

    public DateTimeOffset LastModified { get; private set; }

    public string? LastModifiedBy { get; private set; }
}
