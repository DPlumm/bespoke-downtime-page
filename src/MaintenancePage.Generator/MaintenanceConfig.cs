using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaintenancePage.Generator;

internal sealed record MaintenanceConfig
{
    [JsonPropertyName("serviceName")]
    public required string ServiceName { get; init; }

    [JsonPropertyName("changeReference")]
    public required string ChangeReference { get; init; }

    [JsonPropertyName("changeLinkUrl")]
    public required string ChangeLinkUrl { get; init; }

    [JsonPropertyName("changeLinkText")]
    public required string ChangeLinkText { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

internal static class MaintenanceConfigLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static MaintenanceConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new ConfigurationLoadException($"Config file not found at '{path}'.");
        }

        MaintenanceConfig? config;

        try
        {
            config = JsonSerializer.Deserialize<MaintenanceConfig>(File.ReadAllText(path), Options);
        }
        catch (JsonException ex)
        {
            throw new ConfigurationLoadException($"Invalid JSON in config file '{path}': {ex.Message}");
        }

        if (config is null)
        {
            throw new ConfigurationLoadException("Unable to read maintenance page configuration.");
        }

        var missingFields = GetMissingFields(config);
        if (missingFields.Count > 0)
        {
            throw new ConfigurationLoadException($"Config is missing required fields: {string.Join(", ", missingFields)}.");
        }

        return config with
        {
            ServiceName = config.ServiceName.Trim(),
            ChangeReference = config.ChangeReference.Trim(),
            ChangeLinkUrl = config.ChangeLinkUrl.Trim(),
            ChangeLinkText = config.ChangeLinkText.Trim(),
            Message = config.Message.Trim()
        };
    }

    private static List<string> GetMissingFields(MaintenanceConfig config)
    {
        var missing = new List<string>();

        ValidateField(config.ServiceName, "serviceName");
        ValidateField(config.ChangeReference, "changeReference");
        ValidateField(config.ChangeLinkUrl, "changeLinkUrl");
        ValidateField(config.ChangeLinkText, "changeLinkText");
        ValidateField(config.Message, "message");

        return missing;

        void ValidateField(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                missing.Add(fieldName);
            }
        }
    }
}

internal sealed class ConfigurationLoadException(string message) : Exception(message);
