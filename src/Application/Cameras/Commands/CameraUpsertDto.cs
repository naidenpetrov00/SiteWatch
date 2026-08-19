using FluentValidation;

namespace Application.Cameras.Commands;

public abstract record CameraUpsertDto
{
    public string Name { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? IpAddress { get; init; }
    public int RtspPort { get; init; }
    public int PtzPort { get; init; }
    public Guid SiteId { get; init; }
}

public class CameraUpsertValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : CameraUpsertDto
{
    public CameraUpsertValidator()
    {
        RuleFor(camera => camera.Name).NotEmpty().MaximumLength(100);
        RuleFor(camera => camera.Brand)
            .NotEmpty()
            .Must(value => Enum.TryParse<Domain.SeedWork.Enums.Brand>(value, true, out var brand)
                && brand != Domain.SeedWork.Enums.Brand.Unknown)
            .WithMessage("Brand must be a supported camera brand.");
        RuleFor(camera => camera.Model).NotEmpty().Length(2, 100);
        RuleFor(camera => camera.Username).MaximumLength(50);
        RuleFor(camera => camera.Password).MaximumLength(50);
        RuleFor(camera => camera.IpAddress).MaximumLength(39);
        RuleFor(camera => camera.RtspPort).InclusiveBetween(1, 65535);
        RuleFor(camera => camera.PtzPort).InclusiveBetween(1, 65535);
        RuleFor(camera => camera.SiteId).NotEmpty();
    }
}
