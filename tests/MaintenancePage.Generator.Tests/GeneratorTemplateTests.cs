using System;
using MaintenancePage.Generator;
using Xunit;

namespace MaintenancePage.Generator.Tests;

public class GeneratorTemplateTests
{
    [Fact]
    public void Rendering_includes_service_name_and_change_reference()
    {
        var config = CreateConfig(
            serviceName: "DocumentStore",
            changeReference: "CR<001>",
            changeLinkUrl: "https://example.com/changes/1",
            changeLinkText: "View change",
            message: "Service will return shortly."
        );

        var template = "<div>{{SERVICE_NAME}}</div><span>{{CHANGE_REFERENCE}}</span>";

        var result = TemplateRenderer.ApplyTemplate(template, config);

        Assert.Contains("Payments &amp; Orders", result);
        Assert.Contains("CR&lt;001&gt;", result);
    }

    [Fact]
    public void Message_with_script_tag_is_encoded()
    {
        var message = "<script>alert('xss')</script> Please standby.";
        var config = CreateConfig(
            serviceName: "DocumentStore",
            changeReference: "CR-9",
            changeLinkUrl: "https://example.com/changes/9",
            changeLinkText: "Details",
            message: message
        );

        var template = "<main>{{MESSAGE_HTML}}</main>";

        var result = TemplateRenderer.ApplyTemplate(template, config);

        Assert.Contains("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt; Please standby.", result);
        Assert.DoesNotContain("<script>", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Invalid_change_link_url_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => TemplateRenderer.ValidateChangeLinkUrl("ftp://example.com"));
        Assert.Throws<ArgumentException>(() => TemplateRenderer.ValidateChangeLinkUrl("javascript:alert(1)"));
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
