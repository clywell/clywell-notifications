namespace Clywell.Core.Notifications;

/// <summary>
/// Primary API for sending notifications. Resolves the appropriate channel and dispatches
/// the notification for delivery.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a single notification.
    /// </summary>
    /// <param name="request">The notification request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the send operation.</returns>
    Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends multiple notifications. Individual failures do not prevent remaining notifications
    /// from being sent.
    /// </summary>
    /// <param name="requests">The notification requests.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result for each request, in the same order.</returns>
    Task<IReadOnlyList<NotificationResult>> SendAsync(IEnumerable<NotificationRequest> requests, CancellationToken cancellationToken = default);
}