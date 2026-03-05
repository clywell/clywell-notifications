namespace Clywell.Core.Notifications.Renderer.Scriban;

/// <summary>
/// Extension methods for registering the Scriban template renderer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Scriban template renderer as an <see cref="ITemplateRenderer"/> implementation.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Consumers must separately register an <see cref="ITemplateProvider"/> implementation
    /// to define where templates are loaded from (e.g., database, file system).
    /// </para>
    /// <example>
    /// <code>
    /// services.AddNotifications()
    ///         .AddScribanRenderer();
    ///
    /// // Register your template provider
    /// services.AddScoped&lt;ITemplateProvider, MyDatabaseTemplateProvider&gt;();
    /// </code>
    /// </example>
    /// </remarks>
    public static IServiceCollection AddScribanRenderer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITemplateRenderer, ScribanTemplateRenderer>();

        return services;
    }
}
