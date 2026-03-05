using Scriban;
using Scriban.Runtime;

namespace Clywell.Core.Notifications.Renderer.Scriban;

/// <summary>
/// Renders notification templates using the Scriban template engine.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ScribanTemplateRenderer"/> class.
/// </remarks>
internal sealed class ScribanTemplateRenderer(ITemplateProvider templateProvider) : ITemplateRenderer
{
    /// <inheritdoc/>
    public async Task<RenderedContent> RenderAsync(
        string templateKey,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentNullException.ThrowIfNull(parameters);

        var definition = await templateProvider.GetTemplateAsync(templateKey, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException($"Template '{templateKey}' not found.");

        var subject = definition.SubjectTemplate is not null
            ? await RenderTemplatePartAsync(definition.SubjectTemplate, parameters, templateKey, "Subject").ConfigureAwait(false)
            : null;

        var htmlBody = definition.HtmlBodyTemplate is not null
            ? await RenderTemplatePartAsync(definition.HtmlBodyTemplate, parameters, templateKey, "HtmlBody").ConfigureAwait(false)
            : null;

        var plainTextBody = definition.PlainTextBodyTemplate is not null
            ? await RenderTemplatePartAsync(definition.PlainTextBodyTemplate, parameters, templateKey, "PlainTextBody").ConfigureAwait(false)
            : null;

        return new RenderedContent(subject, htmlBody, plainTextBody);
    }

    private static async Task<string> RenderTemplatePartAsync(
        string templateSource,
        Dictionary<string, object> parameters,
        string templateKey,
        string partName)
    {
        var template = Template.Parse(templateSource);

        if (template.HasErrors)
        {
            var errors = string.Join("; ", template.Messages);
            throw new InvalidOperationException(
                $"Failed to parse {partName} template for '{templateKey}': {errors}");
        }

        var scriptObject = new ScriptObject();
        foreach (var kvp in parameters)
        {
            scriptObject[kvp.Key] = kvp.Value;
        }

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        return await template.RenderAsync(context).ConfigureAwait(false);
    }
}
