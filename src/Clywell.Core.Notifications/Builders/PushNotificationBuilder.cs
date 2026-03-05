namespace Clywell.Core.Notifications;

/// <summary>
/// Fluent builder for push notifications.
/// </summary>
public sealed class PushNotificationBuilder : NotificationBuilderBase<PushNotificationBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PushNotificationBuilder"/> class.
    /// </summary>
    public PushNotificationBuilder()
        : base(NotificationChannel.Push)
    {
    }

    /// <summary>
    /// Sets a device token recipient.
    /// </summary>
    /// <param name="deviceToken">The device token.</param>
    /// <param name="name">An optional display name.</param>
    /// <returns>The current builder instance.</returns>
    public PushNotificationBuilder ToDevice(string deviceToken, string? name = null)
    {
        _recipient = new NotificationRecipient
        {
            DeviceToken = deviceToken,
            Name = name
        };

        return this;
    }

    /// <summary>
    /// Sets a user recipient.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="name">An optional display name.</param>
    /// <returns>The current builder instance.</returns>
    public PushNotificationBuilder ToUser(string userId, string? name = null)
    {
        _recipient = new NotificationRecipient
        {
            UserId = userId,
            Name = name
        };

        return this;
    }

    /// <summary>
    /// Sets the push notification title.
    /// </summary>
    /// <param name="title">The title text.</param>
    /// <returns>The current builder instance.</returns>
    public PushNotificationBuilder WithTitle(string title)
    {
        _subject = title;
        return this;
    }
}
