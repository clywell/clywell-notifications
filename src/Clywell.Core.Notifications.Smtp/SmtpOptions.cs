namespace Clywell.Core.Notifications.Smtp;

/// <summary>
/// Configuration options for the SMTP notification channel.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>SMTP server hostname.</summary>
    public string Host { get; private set; } = string.Empty;

    /// <summary>SMTP server port. Defaults to 587.</summary>
    public int Port { get; private set; } = 587;

    /// <summary>SMTP authentication username.</summary>
    public string? UserName { get; private set; }

    /// <summary>SMTP authentication password.</summary>
    public string? Password { get; private set; }

    /// <summary>Whether to use SSL/TLS. Defaults to true.</summary>
    public bool UseSsl { get; private set; } = true;

    /// <summary>Sender email address.</summary>
    public string SenderEmail { get; private set; } = string.Empty;

    /// <summary>Sender display name.</summary>
    public string? SenderName { get; private set; }

    /// <summary>Configures the SMTP server host and optionally the port.</summary>
    public SmtpOptions UseHost(string host, int port = 587)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        Host = host;
        Port = port;
        return this;
    }

    /// <summary>Configures SMTP authentication credentials.</summary>
    public SmtpOptions WithCredentials(string userName, string password)
    {
        UserName = userName;
        Password = password;
        return this;
    }

    /// <summary>Enables or disables SSL/TLS.</summary>
    public SmtpOptions WithSsl(bool useSsl = true)
    {
        UseSsl = useSsl;
        return this;
    }

    /// <summary>Configures the sender email address and optional display name.</summary>
    public SmtpOptions UseSender(string email, string? name = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        SenderEmail = email;
        SenderName = name;
        return this;
    }
}
