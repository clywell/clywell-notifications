namespace Clywell.Core.Notifications;

/// <summary>
/// Optional pluggable notification logger. Implement this interface to persist notification
/// delivery results (e.g., to a database, message queue, or audit trail).
/// </summary>
public interface INotificationLogger
{
    /// <summary>
    /// Logs a notification delivery result.
    /// </summary>
    /// <param name="result">The notification result to log.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LogAsync(NotificationResult result, CancellationToken cancellationToken = default);
}