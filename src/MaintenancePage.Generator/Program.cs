using MaintenancePage.Generator;
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

    var content = TemplateRenderer.ApplyTemplate(template, config);

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
