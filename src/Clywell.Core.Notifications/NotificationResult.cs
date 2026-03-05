namespace Clywell.Core.Notifications;

/// <summary>
/// Represents the result of a notification send operation.
/// </summary>
public sealed record NotificationResult
{
    /// <summary>Gets the unique notification identifier.</summary>
    public required string NotificationId { get; init; }

    /// <summary>Gets the delivery status.</summary>
    public required NotificationStatus Status { get; init; }

    /// <summary>Gets the timestamp when the notification was sent.</summary>
    public DateTimeOffset? SentAt { get; init; }

    /// <summary>Gets the error message if delivery failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Creates a successful result.</summary>
    internal static NotificationResult Success(string notificationId) => new()
    {
        NotificationId = notificationId,
        Status = NotificationStatus.Sent,
        SentAt = DateTimeOffset.UtcNow
    };

    /// <summary>Creates a failed result.</summary>
    internal static NotificationResult Failure(string notificationId, string errorMessage) => new()
    {
        NotificationId = notificationId,
        Status = NotificationStatus.Failed,
        ErrorMessage = errorMessage
    };
}