using MaintenancePage.Generator;

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

    Console.WriteLine($"Configuration: {arguments.ConfigPath}");
    Console.WriteLine($"Template:      {arguments.TemplatePath}");
    Console.WriteLine($"Output:        {arguments.OutputPath}");

    var config = MaintenanceConfigLoader.Load(arguments.ConfigPath);
    var template = LoadTemplate(arguments.TemplatePath);

    var content = ApplyTemplate(template, config);
    Directory.CreateDirectory(Path.GetDirectoryName(arguments.OutputPath)!);
    File.WriteAllText(arguments.OutputPath, content);

    Console.WriteLine("Generated maintenance page successfully.");
    return 0;
}

static (string ConfigPath, string TemplatePath, string OutputPath) ParseArguments(string[] args)
{
    var configPath = Path.Combine("config", "maintenance.json");
    var templatePath = Path.Combine("templates", "index.html");
    var outputPath = Path.Combine("dist", "index.html");

    var positional = new List<string>();

    for (var index = 0; index < args.Length; index++)
    {
        var argument = args[index];

        switch (argument)
        {
            case "--config":
            case "-c":
                configPath = RequireNextValue(args, ref index, argument);
                break;
            case "--template":
            case "-t":
                templatePath = RequireNextValue(args, ref index, argument);
                break;
            case "--output":
            case "-o":
                outputPath = RequireNextValue(args, ref index, argument);
                break;
            default:
                positional.Add(argument);
                break;
        }
    }

    if (positional.Count > 0)
    {
        configPath = positional[0];
    }

    if (positional.Count > 1)
    {
        templatePath = positional[1];
    }

    if (positional.Count > 2)
    {
        outputPath = positional[2];
    }

    return (configPath, templatePath, outputPath);
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
    var substitutions = new Dictionary<string, string>
    {
        ["title"] = $"{config.ServiceName} maintenance",
        ["serviceName"] = config.ServiceName,
        ["changeReference"] = config.ChangeReference,
        ["changeLinkUrl"] = config.ChangeLinkUrl,
        ["changeLinkText"] = config.ChangeLinkText,
        ["message"] = config.Message.ReplaceLineEndings("<br />\n")
    };

    var result = template;

    foreach (var (key, value) in substitutions)
    {
        var placeholder = $"{{{{{key}}}}}";
        result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
    }

    return result;
}
