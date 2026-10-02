using System.ComponentModel.DataAnnotations.Schema;
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

    public Guid CompanyPersonId { get; private set; }
    public Person CompanyPerson { get; private set; } = null!;
    public int Version { get; private set; }
    public RetailerExtractionProfileStatus Status { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public string? PublishedBy { get; private set; }
    public long ConfigurationRevision { get; private set; }
    public long? ValidatedConfigurationRevision { get; private set; }
    public DateTimeOffset? LastSuccessfulTestAt { get; private set; }
    public Guid? LastSuccessfulTestRuleId { get; private set; }
    public RetailerExtractionRule? LastSuccessfulTestRule { get; private set; }
    public IReadOnlyCollection<RetailerExtractionAllowedHost> AllowedHosts => _allowedHosts;
    public IReadOnlyCollection<RetailerExtractionRule> Rules => _rules;

    public static RetailerExtractionProfile CreateDraft(
        Person companyPerson,
        int version,
        IEnumerable<string> initialHosts)
    {
        ArgumentNullException.ThrowIfNull(companyPerson);
        if (companyPerson.Type != PersonType.Company)
        {
            throw new ArgumentException(
                "A retailer extraction profile must be owned by a company Person.",
                nameof(companyPerson));
        }
        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        var profile = new RetailerExtractionProfile
        {
            Id = Guid.NewGuid(),
            CompanyPerson = companyPerson,
            CompanyPersonId = companyPerson.Id,
            Version = version,
            Status = RetailerExtractionProfileStatus.Draft,
            IsActive = false
        };
        profile.ReplaceAllowedHosts(initialHosts);
        return profile;
    }

    public static RetailerExtractionProfile CloneDraft(
        Person companyPerson,
        RetailerExtractionProfile source,
        int version)
    {
        ArgumentNullException.ThrowIfNull(companyPerson);
        ArgumentNullException.ThrowIfNull(source);
        if (source.CompanyPersonId != companyPerson.Id)
        {
            throw new InvalidOperationException("The source profile belongs to another company Person.");
        }
        if (source.Status != RetailerExtractionProfileStatus.Published)
        {
            throw new InvalidOperationException("Only a published profile can be cloned.");
        }

        var draft = CreateDraft(
            companyPerson,
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
        MarkConfigurationChanged();
    }

    public RetailerExtractionRule AddRule(RetailerExtractionRuleConfiguration configuration)
    {
        EnsureDraft();
        var rule = RetailerExtractionRule.Create(this, _rules.Count + 1, configuration);
        _rules.Add(rule);
        MarkConfigurationChanged();
        return rule;
    }

    public void UpdateRule(Guid ruleId, RetailerExtractionRuleConfiguration configuration)
    {
        EnsureDraft();
        FindRule(ruleId).Update(configuration);
        MarkConfigurationChanged();
    }

    public void SetRuleEnabled(Guid ruleId, bool isEnabled)
    {
        EnsureDraft();
        FindRule(ruleId).SetEnabled(isEnabled);
        MarkConfigurationChanged();
    }

    public void RemoveRule(Guid ruleId)
    {
        EnsureDraft();
        _rules.Remove(FindRule(ruleId));
        NormalizePriorities();
        MarkConfigurationChanged();
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

        MarkConfigurationChanged();
    }

    public void RecordSuccessfulTest(
        long testedConfigurationRevision,
        DateTimeOffset testedAt,
        RetailerExtractionRule matchedRule)
    {
        EnsureDraft();
        ArgumentNullException.ThrowIfNull(matchedRule);
        if (testedConfigurationRevision != ConfigurationRevision)
        {
            throw new InvalidOperationException(
                "The extraction profile changed while the test was running.");
        }
        if (testedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The test time must be UTC.", nameof(testedAt));
        }
        if (matchedRule.ExtractionProfileId != Id || !matchedRule.IsEnabled)
        {
            throw new InvalidOperationException(
                "The successful rule must be enabled and belong to this profile.");
        }

        ValidatedConfigurationRevision = ConfigurationRevision;
        LastSuccessfulTestAt = testedAt;
        LastSuccessfulTestRule = matchedRule;
        LastSuccessfulTestRuleId = matchedRule.Id;
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
        if (!IsCurrentConfigurationValidated)
        {
            throw new InvalidOperationException(
                "The current saved profile configuration must pass an extraction test before publishing.");
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

    [NotMapped]
    public bool IsCurrentConfigurationValidated =>
        ConfigurationRevision > 0
        && ValidatedConfigurationRevision == ConfigurationRevision
        && LastSuccessfulTestAt.HasValue
        && LastSuccessfulTestRuleId.HasValue;

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

    private void MarkConfigurationChanged()
    {
        ConfigurationRevision = checked(ConfigurationRevision + 1);
        ValidatedConfigurationRevision = null;
        LastSuccessfulTestAt = null;
        LastSuccessfulTestRuleId = null;
        LastSuccessfulTestRule = null;
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
