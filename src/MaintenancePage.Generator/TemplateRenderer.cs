using System.Net;
using System.Text;

namespace MaintenancePage.Generator;

public static class TemplateRenderer
{
    public static string ApplyTemplate(string template, MaintenanceConfig config)
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

    public static string ValidateChangeLinkUrl(string changeLinkUrl)
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

    private static string BuildMessageHtml(string message)
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
}
