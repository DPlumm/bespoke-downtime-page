using System.Text.Json;

var configPath = args.ElementAtOrDefault(0) ?? Path.Combine("config", "config.json");
var templatePath = args.ElementAtOrDefault(1) ?? Path.Combine("templates", "index.html");
var outputPath = args.ElementAtOrDefault(2) ?? Path.Combine("dist", "index.html");

Console.WriteLine($"Configuration: {configPath}");
Console.WriteLine($"Template:      {templatePath}");
Console.WriteLine($"Output:        {outputPath}");

if (!File.Exists(configPath))
{
    Console.Error.WriteLine($"Config file not found at '{configPath}'.");
    return 1;
}

if (!File.Exists(templatePath))
{
    Console.Error.WriteLine($"Template file not found at '{templatePath}'.");
    return 1;
}

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};

var config = JsonSerializer.Deserialize<MaintenanceConfig>(File.ReadAllText(configPath), options);
if (config is null)
{
    Console.Error.WriteLine("Unable to read maintenance page configuration.");
    return 1;
}

var template = File.ReadAllText(templatePath);
var substitutions = new Dictionary<string, string>
{
    ["title"] = config.Title,
    ["message"] = config.Message,
    ["estimatedReturn"] = config.EstimatedReturn ?? string.Empty,
    ["contactEmail"] = config.ContactEmail ?? string.Empty
};

var content = ApplyTemplate(template, substitutions);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, content);

Console.WriteLine("Generated maintenance page successfully.");
return 0;

static string ApplyTemplate(string template, IReadOnlyDictionary<string, string> values)
{
    var result = template;

    foreach (var (key, value) in values)
    {
        var placeholder = $"{{{{{key}}}}}";
        result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
    }

    return result;
}

internal sealed record MaintenanceConfig
{
    public required string Title { get; init; }

    public required string Message { get; init; }

    public string? EstimatedReturn { get; init; }

    public string? ContactEmail { get; init; }
}
