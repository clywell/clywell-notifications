namespace Clywell.Core.Notifications;

/// <summary>
/// Fluent builder for email notifications.
/// </summary>
public sealed class EmailNotificationBuilder : NotificationBuilderBase<EmailNotificationBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailNotificationBuilder"/> class.
    /// </summary>
    public EmailNotificationBuilder()
        : base(NotificationChannel.Email)
    {
    }

    /// <summary>
    /// Sets the email recipient.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="name">An optional display name.</param>
    /// <returns>The current builder instance.</returns>
    public EmailNotificationBuilder To(string email, string? name = null)
    {
        _recipient = new NotificationRecipient
        {
            Email = email,
            Name = name
        };

        return this;
    }

    /// <summary>
    /// Sets the email subject.
    /// </summary>
    /// <param name="subject">The subject text.</param>
    /// <returns>The current builder instance.</returns>
    public EmailNotificationBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }
}
