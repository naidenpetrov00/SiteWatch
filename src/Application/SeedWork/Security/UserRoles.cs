namespace Application.SeedWork.Security;

public static class UserRoles
{
    public const string Administrator = "Administrator";
    public const string Client = "Client";
    public const string Worker = "Worker";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        [Administrator, Client, Worker],
        StringComparer.Ordinal
    );

    public static bool IsSupported(string? role) =>
        role is not null && All.Contains(role);
}

public static class UserRoleGroups
{
    public const string AdministratorOrWorker =
        UserRoles.Administrator + "," + UserRoles.Worker;
}
