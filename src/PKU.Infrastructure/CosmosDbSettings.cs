namespace PKU.Infrastructure;

public class CosmosDbSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string UsersContainerName { get; set; } = "users";
}
