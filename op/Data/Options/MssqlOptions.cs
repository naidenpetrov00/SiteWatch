namespace Infrastructure.Data.Options;

public class MssqlOptions
{
    public string? ConnectionString { get; set; }
    public string? ConnectionStringDockerDb { get; set; }
    public string? ConnectionStringDotNetBuildWithDockerDb { get; set; }
}
