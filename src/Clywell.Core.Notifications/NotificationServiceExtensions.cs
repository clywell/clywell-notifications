namespace Clywell.Core.Notifications;

/// <summary>
/// Extension methods for sending notifications with fluent builders.
/// </summary>
public static class NotificationServiceExtensions
{
    /// <summary>
    /// Sends an email notification configured with <see cref="EmailNotificationBuilder"/>.
    /// </summary>
    /// <param name="service">The notification service.</param>
    /// <param name="configure">The email builder configuration action.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the send operation.</returns>
    public static Task<NotificationResult> SendEmailAsync(
        this INotificationService service,
        Action<EmailNotificationBuilder> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new EmailNotificationBuilder();
        configure(builder);
        return service.SendAsync(builder.Build(), cancellationToken);
    }

    /// <summary>
    /// Sends an SMS notification configured with <see cref="SmsNotificationBuilder"/>.
    /// </summary>
    /// <param name="service">The notification service.</param>
    /// <param name="configure">The SMS builder configuration action.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the send operation.</returns>
    public static Task<NotificationResult> SendSmsAsync(
        this INotificationService service,
        Action<SmsNotificationBuilder> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new SmsNotificationBuilder();
        configure(builder);
        return service.SendAsync(builder.Build(), cancellationToken);
    }

    /// <summary>
    /// Sends a push notification configured with <see cref="PushNotificationBuilder"/>.
    /// </summary>
    /// <param name="service">The notification service.</param>
    /// <param name="configure">The push builder configuration action.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the send operation.</returns>
    public static Task<NotificationResult> SendPushAsync(
        this INotificationService service,
        Action<PushNotificationBuilder> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new PushNotificationBuilder();
        configure(builder);
        return service.SendAsync(builder.Build(), cancellationToken);
    }

    /// <summary>
    /// Sends an in-app notification configured with <see cref="InAppNotificationBuilder"/>.
    /// </summary>
    /// <param name="service">The notification service.</param>
    /// <param name="configure">The in-app builder configuration action.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the send operation.</returns>
    public static Task<NotificationResult> SendInAppAsync(
        this INotificationService service,
        Action<InAppNotificationBuilder> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new InAppNotificationBuilder();
        configure(builder);
        return service.SendAsync(builder.Build(), cancellationToken);
    }

    /// <summary>
    /// Sends a notification configured through the root <see cref="NotificationBuilder"/> selector.
    /// </summary>
    /// <param name="service">The notification service.</param>
    /// <param name="configure">A factory function that creates a channel-specific builder.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the send operation.</returns>
    public static Task<NotificationResult> SendAsync(
        this INotificationService service,
        Func<INotificationBuilder> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = configure();
        return service.SendAsync(builder.Build(), cancellationToken);
    }
}
