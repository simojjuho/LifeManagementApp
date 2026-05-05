namespace LifeManagementTool.Database;

public static class DataUtility
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DevDbConnection");
        return connectionString;
    }
}