namespace Clywell.Core.Notifications;

/// <summary>
/// Renders notification templates with parameter substitution.
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Renders a notification template identified by key, substituting the provided parameters.
    /// </summary>
    /// <param name="templateKey">The template identifier.</param>
    /// <param name="parameters">The template parameters for variable substitution.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The rendered content.</returns>
    Task<RenderedContent> RenderAsync(string templateKey, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
}