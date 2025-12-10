using Domain.SeedWork;

namespace Domain.Entities;

public sealed class CameraBrands : BaseEntity
{
    public CameraBrands()
    {
    }

    public Brand Type { get; set; }
}