using Domain.SeedWork;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class RetailerExtractionProfile : BaseAuditableEntity, IAgregateRoot
{
    private readonly List<RetailerExtractionAllowedHost> _allowedHosts = [];
    private readonly List<RetailerExtractionRule> _rules = [];

    private RetailerExtractionProfile()
    {
    }

    public Guid RetailerId { get; private set; }
    public Retailer Retailer { get; private set; } = null!;
    public int Version { get; private set; }
    public RetailerExtractionProfileStatus Status { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public string? PublishedBy { get; private set; }
    public IReadOnlyCollection<RetailerExtractionAllowedHost> AllowedHosts => _allowedHosts;
    public IReadOnlyCollection<RetailerExtractionRule> Rules => _rules;

    public static RetailerExtractionProfile CreateDraft(
        Retailer retailer,
        int version,
        IEnumerable<string> initialHosts)
    {
        ArgumentNullException.ThrowIfNull(retailer);
        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        var profile = new RetailerExtractionProfile
        {
            Id = Guid.NewGuid(),
            Retailer = retailer,
            RetailerId = retailer.Id,
            Version = version,
            Status = RetailerExtractionProfileStatus.Draft,
            IsActive = false
        };
        profile.ReplaceAllowedHosts(initialHosts);
        return profile;
    }

    public static RetailerExtractionProfile CloneDraft(
        Retailer retailer,
        RetailerExtractionProfile source,
        int version)
    {
        ArgumentNullException.ThrowIfNull(retailer);
        ArgumentNullException.ThrowIfNull(source);
        if (source.RetailerId != retailer.Id)
        {
            throw new InvalidOperationException("The source profile belongs to another retailer.");
        }
        if (source.Status != RetailerExtractionProfileStatus.Published)
        {
            throw new InvalidOperationException("Only a published profile can be cloned.");
        }

        var draft = CreateDraft(
            retailer,
            version,
            source.AllowedHosts.Select(host => host.NormalizedHost));
        foreach (var rule in source.Rules.OrderBy(rule => rule.Priority))
        {
            draft._rules.Add(rule.CloneFor(draft, draft._rules.Count + 1));
        }

        return draft;
    }

    public void ReplaceAllowedHosts(IEnumerable<string> hosts)
    {
        EnsureDraft();
        ArgumentNullException.ThrowIfNull(hosts);
        var normalized = hosts
            .Select(host => RetailerExtractionHost.Create(host).Value)
            .ToList();
        if (normalized.Count != normalized.Distinct(StringComparer.Ordinal).Count())
        {
            throw new ArgumentException("Allowed hosts must be unique after normalization.");
        }

        _allowedHosts.Clear();
        _allowedHosts.AddRange(normalized.Select(host =>
            RetailerExtractionAllowedHost.Create(this, host)));
    }

    public RetailerExtractionRule AddRule(RetailerExtractionRuleConfiguration configuration)
    {
        EnsureDraft();
        var rule = RetailerExtractionRule.Create(this, _rules.Count + 1, configuration);
        _rules.Add(rule);
        return rule;
    }

    public void UpdateRule(Guid ruleId, RetailerExtractionRuleConfiguration configuration)
    {
        EnsureDraft();
        FindRule(ruleId).Update(configuration);
    }

    public void SetRuleEnabled(Guid ruleId, bool isEnabled)
    {
        EnsureDraft();
        FindRule(ruleId).SetEnabled(isEnabled);
    }

    public void RemoveRule(Guid ruleId)
    {
        EnsureDraft();
        _rules.Remove(FindRule(ruleId));
        NormalizePriorities();
    }

    public void ReorderRules(IReadOnlyList<Guid> orderedRuleIds)
    {
        EnsureDraft();
        ArgumentNullException.ThrowIfNull(orderedRuleIds);
        if (orderedRuleIds.Count != _rules.Count
            || orderedRuleIds.Distinct().Count() != _rules.Count
            || orderedRuleIds.Any(id => _rules.All(rule => rule.Id != id)))
        {
            throw new ArgumentException("The ordered rule IDs must contain every draft rule exactly once.");
        }

        for (var index = 0; index < orderedRuleIds.Count; index++)
        {
            FindRule(orderedRuleIds[index]).SetPriority(index + 1);
        }
    }

    public void Publish(DateTimeOffset publishedAt, string publishedBy)
    {
        EnsureDraft();
        ArgumentException.ThrowIfNullOrWhiteSpace(publishedBy);
        if (publishedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Publication time must be UTC.", nameof(publishedAt));
        }
        if (_allowedHosts.Count == 0)
        {
            throw new InvalidOperationException("A profile requires at least one allowed host before publishing.");
        }
        if (_rules.All(rule => !rule.IsEnabled))
        {
            throw new InvalidOperationException("A profile requires at least one enabled rule before publishing.");
        }

        Status = RetailerExtractionProfileStatus.Published;
        PublishedAt = publishedAt;
        PublishedBy = publishedBy.Trim();
        IsActive = true;
    }

    public void Activate()
    {
        EnsurePublished();
        IsActive = true;
    }

    public void Deactivate()
    {
        EnsurePublished();
        IsActive = false;
    }

    public void EnsureCanDelete() => EnsureDraft();

    private RetailerExtractionRule FindRule(Guid ruleId) =>
        _rules.SingleOrDefault(rule => rule.Id == ruleId)
        ?? throw new KeyNotFoundException("The extraction rule does not belong to this profile.");

    private void NormalizePriorities()
    {
        var ordered = _rules.OrderBy(rule => rule.Priority).ThenBy(rule => rule.Id).ToList();
        for (var index = 0; index < ordered.Count; index++)
        {
            ordered[index].SetPriority(index + 1);
        }
    }

    private void EnsureDraft()
    {
        if (Status != RetailerExtractionProfileStatus.Draft
            || IsActive
            || PublishedAt.HasValue
            || PublishedBy is not null)
        {
            throw new InvalidOperationException("Only a consistent draft profile can be changed.");
        }
    }

    private void EnsurePublished()
    {
        if (Status != RetailerExtractionProfileStatus.Published
            || !PublishedAt.HasValue
            || string.IsNullOrWhiteSpace(PublishedBy))
        {
            throw new InvalidOperationException("Only a published profile can be activated.");
        }
    }
}
