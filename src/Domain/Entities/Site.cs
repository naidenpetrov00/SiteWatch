using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Site : BaseAuditableEntity
{
    private readonly HashSet<ApplicationUser> _users = [];
    private readonly HashSet<Camera> _cameras = [];
    private readonly HashSet<SiteImage> _images = [];
    private readonly HashSet<SiteFile> _files = [];
    private readonly HashSet<SiteVideo> _videos = [];

    public Site(SiteName name, SiteAddress address, SiteMediaPolicy? mediaPolicy = null)
    {
        Name = name;
        Address = address;
        MediaPolicy = mediaPolicy ?? SiteMediaPolicy.Regular();
    }

    // ReSharper disable once UnusedMember.Local
    private Site()
    {
    }

    public SiteName Name { get; private set; } = null!;
    public SiteAddress Address { get; private set; } = null!;
    public SiteMediaPolicy MediaPolicy { get; private set; } = null!;
    public IReadOnlyCollection<ApplicationUser> Users => _users;
    public IReadOnlyCollection<Camera> Cameras => _cameras;
    public IReadOnlyCollection<SiteImage> Images => _images;
    public IReadOnlyCollection<SiteFile> Files => _files;
    public IReadOnlyCollection<SiteVideo> Videos => _videos;

    public void ChangeMediaPolicy(SiteMediaPolicy mediaPolicy) =>
        MediaPolicy = Guard.Against.Null(mediaPolicy);

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
