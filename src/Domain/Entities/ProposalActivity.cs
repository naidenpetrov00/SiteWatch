using Domain.SeedWork;

namespace Domain.Entities;

public sealed class ProposalActivity : BaseEntity
{
    private readonly List<ProposalActivitySection> _sections = [];

    private ProposalActivity()
    {
    }

    public Guid ProposalId { get; private set; }
    public Proposal Proposal { get; private set; } = null!;
    public Guid SourceOfferActivityId { get; private set; }
    public int ActivityNumberId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public IReadOnlyCollection<ProposalActivitySection> Sections => _sections;

    internal static ProposalActivity Create(Proposal proposal, OfferActivity source)
    {
        if (source.Sections.Count == 0)
        {
            throw new InvalidOperationException(
                "Every Proposal activity requires at least one priced section.");
        }

        var activity = new ProposalActivity
        {
            Id = Guid.NewGuid(),
            Proposal = proposal,
            ProposalId = proposal.Id,
            SourceOfferActivityId = source.Id,
            ActivityNumberId = source.ActivityNumberId,
            Name = source.Name,
            Description = source.Description,
            SortOrder = source.SortOrder
        };

        foreach (var section in source.Sections
                     .OrderBy(item => item.SortOrder)
                     .ThenBy(item => item.Id))
        {
            activity._sections.Add(ProposalActivitySection.Create(activity, section));
        }

        return activity;
    }
}
