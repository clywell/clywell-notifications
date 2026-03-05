namespace Clywell.Core.Notifications.SignalR;

/// <summary>
/// Extension methods for registering the SignalR notification channel.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SignalR InApp notification channel.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="SignalROptions"/>.
    /// <code>
    /// services.AddNotifications()
    ///         .AddNotificationsSignalR(options =&gt; options
    ///             .WithMethodName("OnNotification")
    ///             .UseUserAddressing());
    /// </code>
    /// Callers must also call <c>builder.Services.AddSignalR()</c> and map the hub:
    /// <code>app.MapHub&lt;NotificationHub&gt;("/hubs/notifications");</code>
    /// </param>
    public static IServiceCollection AddNotificationsSignalR(this IServiceCollection services, Action<SignalROptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new SignalROptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddScoped<INotificationChannel, SignalRNotificationChannel>();

        return services;
    }
}
