namespace Clywell.Core.Notifications;

/// <summary>
/// Defines the priority level of a notification.
/// </summary>
public enum NotificationPriority
{
    /// <summary>Normal priority — default for most notifications.</summary>
    Normal = 0,

    /// <summary>Critical priority — for urgent, time-sensitive notifications.</summary>
    Critical = 1
}