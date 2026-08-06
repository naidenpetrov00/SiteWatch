namespace Application.SeedWork.Security;

public static class AuthorizationPolicies
{
    public const string Administrator = "AdministratorOnly";
    public const string AdministratorOrWorker = "AdministratorOrWorker";
}
