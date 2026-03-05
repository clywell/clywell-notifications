using MailKit.Net.Smtp;

namespace Clywell.Core.Notifications.Smtp;

/// <summary>
/// Factory for creating SMTP client instances.
/// </summary>
public interface ISmtpClientFactory
{
    /// <summary>Creates a new SMTP client instance.</summary>
    ISmtpClient Create();
}