using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Site : BaseAuditableEntity, IHasNumberId
{
    private readonly HashSet<ApplicationUser> _users = [];
    private readonly HashSet<Camera> _cameras = [];
    private readonly HashSet<SiteImage> _images = [];
    private readonly HashSet<SiteFile> _files = [];
    private readonly HashSet<SiteVideo> _videos = [];
    private readonly HashSet<SitePayment> _payments = [];

    public Site(
        SiteName name,
        SiteAddress address,
        string managerId,
        DateOnly startDate,
        SiteStatus status = SiteStatus.Planning,
        DateOnly? endDate = null,
        SiteMediaPolicy? mediaPolicy = null)
    {
        Name = name;
        Address = address;
        ManagerId = Guard.Against.NullOrWhiteSpace(managerId);
        StartDate = startDate;
        EndDate = ValidateEndDate(startDate, endDate);
        Status = status;
        MediaPolicy = mediaPolicy ?? SiteMediaPolicy.Regular();
    }

    // ReSharper disable once UnusedMember.Local
    private Site()
    {
    }

    public SiteName Name { get; private set; } = null!;
    public int NumberId { get; private set; }
    public SiteAddress Address { get; private set; } = null!;
    public SiteMediaPolicy MediaPolicy { get; private set; } = null!;
    public string ManagerId { get; private set; } = null!;
    public ApplicationUser Manager { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public SiteStatus Status { get; private set; }
    public IReadOnlyCollection<ApplicationUser> Users => _users;
    public IReadOnlyCollection<Camera> Cameras => _cameras;
    public IReadOnlyCollection<SiteImage> Images => _images;
    public IReadOnlyCollection<SiteFile> Files => _files;
    public IReadOnlyCollection<SiteVideo> Videos => _videos;
    public IReadOnlyCollection<SitePayment> Payments => _payments;

    public void ChangeMediaPolicy(SiteMediaPolicy mediaPolicy) =>
        MediaPolicy = Guard.Against.Null(mediaPolicy);

    public void UpdateDetails(
        string name,
        string address,
        string managerId,
        DateOnly startDate,
        DateOnly? endDate,
        SiteStatus status,
        MediaPolicyPreset mediaPolicyPreset)
    {
        Name = name;
        Address = address;
        ManagerId = Guard.Against.NullOrWhiteSpace(managerId);
        StartDate = startDate;
        EndDate = ValidateEndDate(startDate, endDate);
        Status = status;
        MediaPolicy.ChangePreset(mediaPolicyPreset);
    }

    private static DateOnly? ValidateEndDate(DateOnly startDate, DateOnly? endDate)
    {
        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));
        }

        return endDate;
    }

    public void AddImage(SiteImage image) => _images.Add(image);
    public void RemoveImage(SiteImage image) => _images.Remove(image);
    public void AddFile(SiteFile file) => _files.Add(file);
    public void RemoveFile(SiteFile file) => _files.Remove(file);
    public void AddVideo(SiteVideo video) => _videos.Add(video);
    public void RemoveVideo(SiteVideo video) => _videos.Remove(video);
    public void AddUser(ApplicationUser user) => _users.Add(user);

    public void AddUserRange(List<ApplicationUser> users)
    {
        foreach (var user in users)
        {
            _users.Add(user);
        }
    }
}
