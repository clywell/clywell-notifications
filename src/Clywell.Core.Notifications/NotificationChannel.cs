namespace Clywell.Core.Notifications;

/// <summary>
/// Supported notification delivery channels.
/// </summary>
public enum NotificationChannel
{
    /// <summary>Email delivery.</summary>
    Email,

    /// <summary>SMS text message delivery.</summary>
    Sms,

    /// <summary>Push notification delivery.</summary>
    Push,

    /// <summary>In-application notification.</summary>
    InApp
}