namespace Clywell.Core.Notifications.Renderer.Scriban;

/// <summary>
/// Represents the raw template content for rendering.
/// </summary>
/// <param name="SubjectTemplate">The Scriban template for the subject line.</param>
/// <param name="HtmlBodyTemplate">The Scriban template for the HTML body.</param>
/// <param name="PlainTextBodyTemplate">The Scriban template for the plain text body.</param>
public sealed record TemplateDefinition(
    string? SubjectTemplate,
    string? HtmlBodyTemplate,
    string? PlainTextBodyTemplate);
