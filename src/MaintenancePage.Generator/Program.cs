using MaintenancePage.Generator;
using System.Net;
using System.Text;

try
{
    return Run(args);
}
catch (ConfigurationLoadException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static int Run(string[] args)
{
    var arguments = ParseArguments(args);
    var templatePath = Path.Combine("templates", "page.template.html");
    var stylesPath = Path.Combine("templates", "styles.css");

    Console.WriteLine($"Configuration: {arguments.ConfigPath}");
    Console.WriteLine($"Template:      {templatePath}");
    Console.WriteLine($"Styles:        {stylesPath}");
    Console.WriteLine($"Output Dir:    {arguments.OutputDirectory}");

    var config = MaintenanceConfigLoader.Load(arguments.ConfigPath);
    var template = LoadTemplate(templatePath);
    var styles = LoadTemplate(stylesPath);

    var content = ApplyTemplate(template, config);

    Directory.CreateDirectory(arguments.OutputDirectory);
    var htmlOutputPath = Path.Combine(arguments.OutputDirectory, "index.html");
    var cssOutputPath = Path.Combine(arguments.OutputDirectory, "styles.css");

    File.WriteAllText(htmlOutputPath, content, Encoding.UTF8);
    File.WriteAllText(cssOutputPath, styles, Encoding.UTF8);

    Console.WriteLine("Generated maintenance page successfully.");
    return 0;
}

static (string ConfigPath, string OutputDirectory) ParseArguments(string[] args)
{
    var configPath = Path.Combine("config", "maintenance.json");
    var outputDirectory = "dist";

    for (var index = 0; index < args.Length; index++)
    {
        var argument = args[index];

        switch (argument)
        {
            case "--config":
            case "-c":
                configPath = RequireNextValue(args, ref index, argument);
                break;
            case "--out":
                outputDirectory = RequireNextValue(args, ref index, argument);
                break;
            default:
                throw new ArgumentException($"Unknown argument '{argument}'.");
        }
    }

    return (configPath, outputDirectory);
}

static string RequireNextValue(string[] args, ref int currentIndex, string option)
{
    if (currentIndex + 1 >= args.Length)
    {
        throw new ArgumentException($"Missing value for {option}.");
    }

    currentIndex += 1;
    return args[currentIndex];
}

static string LoadTemplate(string templatePath)
{
    if (!File.Exists(templatePath))
    {
        throw new ConfigurationLoadException($"Template file not found at '{templatePath}'.");
    }

    return File.ReadAllText(templatePath);
}

static string ApplyTemplate(string template, MaintenanceConfig config)
{
    var lastUpdated = DateTimeOffset.Now.ToString("O");
    var encodedServiceName = WebUtility.HtmlEncode(config.ServiceName);
    var encodedChangeReference = WebUtility.HtmlEncode(config.ChangeReference);
    var encodedChangeLinkText = WebUtility.HtmlEncode(config.ChangeLinkText);
    var changeLinkUrl = ValidateChangeLinkUrl(config.ChangeLinkUrl);
    var messageHtml = BuildMessageHtml(config.Message);

    var substitutions = new Dictionary<string, string>
    {
        ["SERVICE_NAME"] = encodedServiceName,
        ["CHANGE_REFERENCE"] = encodedChangeReference,
        ["CHANGE_LINK_URL"] = WebUtility.HtmlEncode(changeLinkUrl),
        ["CHANGE_LINK_TEXT"] = encodedChangeLinkText,
        ["MESSAGE_HTML"] = messageHtml,
        ["LAST_UPDATED"] = WebUtility.HtmlEncode(lastUpdated)
    };

    var result = template;

    foreach (var (key, value) in substitutions)
    {
        var placeholder = $"{{{{{key}}}}}";
        result = result.Replace(placeholder, value, StringComparison.Ordinal);
    }

    return result;
}

static string ValidateChangeLinkUrl(string changeLinkUrl)
{
    var trimmed = changeLinkUrl.Trim();

    if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
    {
        if (absoluteUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            return absoluteUri.ToString();
        }

        throw new ArgumentException("changeLinkUrl must use https:// when specifying an absolute URL.");
    }

    if (trimmed.StartsWith('/') && !trimmed.StartsWith("//"))
    {
        return trimmed;
    }

    throw new ArgumentException("changeLinkUrl must be an https:// URL or a relative path starting with '/'.");
}

static string BuildMessageHtml(string message)
{
    var normalized = message.Replace("\r\n", "\n").Replace("\r", "\n");
    var paragraphs = normalized.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (paragraphs.Length == 0)
    {
        return string.Empty;
    }

    var builder = new StringBuilder();

    foreach (var paragraph in paragraphs)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }

        var encodedParagraph = WebUtility.HtmlEncode(paragraph);
        var paragraphHtml = encodedParagraph.Replace("\n", "<br />\n");
        builder.Append("<p>");
        builder.Append(paragraphHtml);
        builder.Append("</p>");
    }

    return builder.ToString();
}
