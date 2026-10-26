using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Hackatohm.Infrastructure.Persistence;

public static class DatabaseConnectionString
{
    public static string GetRequired(IConfiguration configuration, string startDirectory)
    {
        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
            return configuredConnectionString;

        var envValues = ReadEnvFile(startDirectory);
        var database = GetValue("POSTGRES_DB");
        var username = GetValue("POSTGRES_USER");
        var password = GetValue("POSTGRES_PASSWORD");

        if (string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Não foi possível montar a conexão com PostgreSQL. Configure " +
                "ConnectionStrings:DefaultConnection ou POSTGRES_DB, POSTGRES_USER e " +
                "POSTGRES_PASSWORD no arquivo .env da pasta Backend.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = GetValue("POSTGRES_HOST", "localhost"),
            Port = int.TryParse(GetValue("POSTGRES_PORT", "5432"), out var port) ? port : 5432,
            Database = database,
            Username = username,
            Password = password
        }.ConnectionString;

        string GetValue(string key, string? defaultValue = null) =>
            Environment.GetEnvironmentVariable(key)
            ?? configuration[key]
            ?? envValues.GetValueOrDefault(key)
            ?? defaultValue
            ?? string.Empty;
    }

    private static Dictionary<string, string> ReadEnvFile(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            var envPath = Path.Combine(directory.FullName, ".env");
            if (File.Exists(envPath))
            {
                return File.ReadLines(envPath)
                    .Select(ParseEnvLine)
                    .Where(entry => entry is not null)
                    .ToDictionary(entry => entry!.Value.Key, entry => entry!.Value.Value);
            }

            directory = directory.Parent;
        }

        return new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static KeyValuePair<string, string>? ParseEnvLine(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            return null;

        if (trimmed.StartsWith("export ", StringComparison.Ordinal))
            trimmed = trimmed["export ".Length..].TrimStart();

        var separatorIndex = trimmed.IndexOf('=');
        if (separatorIndex <= 0)
            return null;

        var key = trimmed[..separatorIndex].Trim();
        var value = trimmed[(separatorIndex + 1)..].Trim();
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') ||
             (value[0] == '\'' && value[^1] == '\'')))
        {
            value = value[1..^1];
        }

        return new KeyValuePair<string, string>(key, value);
    }
}
