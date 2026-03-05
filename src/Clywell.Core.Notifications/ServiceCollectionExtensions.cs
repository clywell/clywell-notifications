namespace Clywell.Core.Notifications;

/// <summary>
/// Extension methods for registering notification services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core notification services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="NotificationOptions"/>.
    /// <code>
    /// services.AddNotifications(options =&gt; options
    ///     .UseDefaultChannel(NotificationChannel.Email)
    ///     .WithMaxRetryAttempts(5));
    /// </code>
    /// After calling this, register at least one channel provider (e.g. <c>AddNotificationsSmtp</c>)
    /// and optionally a template renderer (e.g. <c>AddScribanRenderer</c>).
    /// </param>
    public static IServiceCollection AddNotifications(this IServiceCollection services, Action<NotificationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new NotificationOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.TryAddScoped<INotificationService, NotificationService>();

        return services;
    }
}