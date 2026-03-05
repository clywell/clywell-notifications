namespace Clywell.Core.Notifications.Renderer.Scriban;

/// <summary>
/// Abstraction for loading notification template content from a storage backend.
/// Consumers must implement this interface to define where templates are stored
/// (e.g., database, file system, embedded resources).
/// </summary>
public interface ITemplateProvider
{
    /// <summary>
    /// Retrieves a template definition by key.
    /// </summary>
    /// <param name="templateKey">The template identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The template definition, or <see langword="null"/> if not found.</returns>
    Task<TemplateDefinition?> GetTemplateAsync(string templateKey, CancellationToken cancellationToken = default);
}
