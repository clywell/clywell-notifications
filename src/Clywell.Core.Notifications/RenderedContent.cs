namespace Clywell.Core.Notifications;

/// <summary>
/// Represents rendered notification content after template processing.
/// </summary>
/// <param name="Subject">The rendered subject line (for email).</param>
/// <param name="HtmlBody">The rendered HTML body.</param>
/// <param name="PlainTextBody">The rendered plain text body.</param>
public sealed record RenderedContent(string? Subject, string? HtmlBody, string? PlainTextBody);