using MailKit.Net.Smtp;

namespace Clywell.Core.Notifications.Smtp;

/// <summary>
/// Default factory that creates MailKit SmtpClient instances.
/// </summary>
internal sealed class DefaultSmtpClientFactory : ISmtpClientFactory
{
    /// <inheritdoc/>
    public ISmtpClient Create() => new SmtpClient();
}