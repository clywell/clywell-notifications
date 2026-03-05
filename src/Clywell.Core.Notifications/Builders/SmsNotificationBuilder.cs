namespace Clywell.Core.Notifications;

/// <summary>
/// Fluent builder for SMS notifications.
/// </summary>
public sealed class SmsNotificationBuilder : NotificationBuilderBase<SmsNotificationBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SmsNotificationBuilder"/> class.
    /// </summary>
    public SmsNotificationBuilder()
        : base(NotificationChannel.Sms)
    {
    }

    /// <summary>
    /// Sets the SMS recipient.
    /// </summary>
    /// <param name="phoneNumber">The recipient phone number.</param>
    /// <param name="name">An optional display name.</param>
    /// <returns>The current builder instance.</returns>
    public SmsNotificationBuilder To(string phoneNumber, string? name = null)
    {
        _recipient = new NotificationRecipient
        {
            PhoneNumber = phoneNumber,
            Name = name
        };

        return this;
    }
}
