using FluentValidation;

namespace Application.Sites.Videos.Queries;

public class GetVideoSnapshotValidator : AbstractValidator<GetVideoSnapshotQuery>
{
    public GetVideoSnapshotValidator()
    {
        RuleFor(x => x.SnapshotId).NotEmpty();
    }
}
