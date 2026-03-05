namespace Clywell.Core.Notifications.Sse;

/// <summary>
/// Extension methods for registering the SSE notification channel.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SSE InApp notification channel.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="SseOptions"/>.
    /// <code>
    /// services.AddNotifications()
    ///         .AddNotificationsSse(options =&gt; options
    ///             .WithEventName("app-notification")
    ///             .UseUserAddressing());
    /// </code>
    /// Callers must also map the SSE endpoint in their middleware pipeline.
    /// </param>
    public static IServiceCollection AddNotificationsSse(this IServiceCollection services, Action<SseOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new SseOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.TryAddSingleton<ISseConnectionManager, SseConnectionManager>();
        services.AddScoped<INotificationChannel, SseNotificationChannel>();

        return services;
    }
}
