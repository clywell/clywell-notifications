namespace Clywell.Core.Notifications;

/// <summary>
/// Notification delivery status.
/// </summary>
public enum NotificationStatus
{
    /// <summary>Notification created but not yet queued.</summary>
    Pending,

    /// <summary>Notification queued for delivery.</summary>
    Queued,

    /// <summary>Notification sent to provider.</summary>
    Sent,

    /// <summary>Notification confirmed delivered.</summary>
    Delivered,

    /// <summary>Notification delivery failed.</summary>
    Failed,

    /// <summary>Notification was cancelled before delivery.</summary>
    Cancelled
}