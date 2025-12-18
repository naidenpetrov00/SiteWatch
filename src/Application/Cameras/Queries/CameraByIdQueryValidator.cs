using Application.SeedWork.Interfaces;
using FluentValidation;

namespace Application.Cameras.Queries;

public class CameraByIdQueryValidator : AbstractValidator<CameraByIdQuery>
{
    private readonly IApplicationDbContext _dbContext;

    public CameraByIdQueryValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        RuleFor(sq => sq.CameraId).NotEmpty()
            .WithMessage("Camera Id is required.");
    }
}