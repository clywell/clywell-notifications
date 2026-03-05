namespace Clywell.Core.Notifications;

/// <summary>
/// Static factory for creating channel-specific notification builders.
/// </summary>
public static class NotificationBuilder
{
    /// <summary>Creates an email notification builder.</summary>
    public static EmailNotificationBuilder ViaEmail() => new EmailNotificationBuilder();

    /// <summary>Creates an SMS notification builder.</summary>
    public static SmsNotificationBuilder ViaSms() => new SmsNotificationBuilder();

    /// <summary>Creates a push notification builder.</summary>
    public static PushNotificationBuilder ViaPush() => new PushNotificationBuilder();

    /// <summary>Creates an in-app notification builder.</summary>
    public static InAppNotificationBuilder ViaInApp() => new InAppNotificationBuilder();
}
