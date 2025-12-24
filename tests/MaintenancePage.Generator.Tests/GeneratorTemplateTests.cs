using System;
using System.Reflection;
using MaintenancePage.Generator;
using Xunit;

namespace MaintenancePage.Generator.Tests;

public class GeneratorTemplateTests
{
    private static readonly Type ProgramType = typeof(MaintenanceConfig).Assembly.GetType("Program")
        ?? throw new InvalidOperationException("Unable to locate Program type.");

    private static readonly MethodInfo ApplyTemplateMethod = ProgramType
        .GetMethod("ApplyTemplate", BindingFlags.Static | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Unable to find ApplyTemplate method.");

    private static readonly MethodInfo ValidateChangeLinkUrlMethod = ProgramType
        .GetMethod("ValidateChangeLinkUrl", BindingFlags.Static | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Unable to find ValidateChangeLinkUrl method.");

    [Fact]
    public void Rendering_includes_service_name_and_change_reference()
    {
        var config = CreateConfig(
            serviceName: "Payments & Orders",
            changeReference: "CR<001>",
            changeLinkUrl: "https://example.com/changes/1",
            changeLinkText: "View change",
            message: "Service will return shortly."
        );

        var template = "<div>{{SERVICE_NAME}}</div><span>{{CHANGE_REFERENCE}}</span>";

        var result = ApplyTemplate(template, config);

        Assert.Contains("Payments &amp; Orders", result);
        Assert.Contains("CR&lt;001&gt;", result);
    }

    [Fact]
    public void Message_with_script_tag_is_encoded()
    {
        var message = "<script>alert('xss')</script> Please standby.";
        var config = CreateConfig(
            serviceName: "Status",
            changeReference: "CR-9",
            changeLinkUrl: "/changes/9",
            changeLinkText: "Details",
            message: message
        );

        var template = "<main>{{MESSAGE_HTML}}</main>";

        var result = ApplyTemplate(template, config);

        Assert.Contains("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt; Please standby.", result);
        Assert.DoesNotContain("<script>", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Invalid_change_link_url_is_rejected()
    {
        static void Invoke(string url)
        {
            ValidateChangeLinkUrlMethod.Invoke(null, new object?[] { url });
        }

        Assert.Throws<TargetInvocationException>(() => Invoke("ftp://example.com"));
        var exception = Assert.Throws<TargetInvocationException>(() => Invoke("javascript:alert(1)"));

        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    private static string ApplyTemplate(string template, MaintenanceConfig config)
    {
        return (string)ApplyTemplateMethod.Invoke(null, new object?[] { template, config })!;
    }

    private static MaintenanceConfig CreateConfig(
        string serviceName,
        string changeReference,
        string changeLinkUrl,
        string changeLinkText,
        string message)
    {
        return new MaintenanceConfig
        {
            ServiceName = serviceName,
            ChangeReference = changeReference,
            ChangeLinkUrl = changeLinkUrl,
            ChangeLinkText = changeLinkText,
            Message = message
        };
    }
}
