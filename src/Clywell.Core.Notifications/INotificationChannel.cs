namespace Clywell.Core.Notifications;

/// <summary>
/// Defines a pluggable notification delivery channel. Implement this interface to add
/// support for a new delivery mechanism (e.g., SMTP, SMS gateway, push notification service).
/// </summary>
public interface INotificationChannel
{
    /// <summary>Gets the channel type this implementation handles.</summary>
    NotificationChannel Channel { get; }

    /// <summary>
    /// Sends a notification message through this channel.
    /// </summary>
    /// <param name="message">The fully resolved notification message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the send operation.</returns>
    Task<NotificationResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}