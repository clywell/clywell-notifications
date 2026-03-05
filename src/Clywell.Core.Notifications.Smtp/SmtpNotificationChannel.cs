using MailKit.Security;
using MimeKit;

namespace Clywell.Core.Notifications.Smtp;

/// <summary>
/// Sends email notifications via SMTP using MailKit.
/// </summary>
internal sealed class SmtpNotificationChannel : INotificationChannel
{
    private readonly SmtpOptions _options;
    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly ILogger<SmtpNotificationChannel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpNotificationChannel"/> class.
    /// </summary>
    public SmtpNotificationChannel(
        SmtpOptions options,
        ISmtpClientFactory smtpClientFactory,
        ILogger<SmtpNotificationChannel> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(smtpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options;
        _smtpClientFactory = smtpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public NotificationChannel Channel => NotificationChannel.Email;

    /// <inheritdoc/>
    public async Task<NotificationResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var notificationId = Guid.NewGuid().ToString("N");

        if (string.IsNullOrWhiteSpace(message.Recipient.Email))
        {
            return NotificationResult.Failure(notificationId, "Recipient email address is required for email notifications.");
        }

        try
        {
            var mimeMessage = BuildMimeMessage(message);

            using var client = _smtpClientFactory.Create();

            var secureSocketOptions = _options.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken).ConfigureAwait(false);
            }

            await client.SendAsync(mimeMessage, cancellationToken).ConfigureAwait(false);
            await client.DisconnectAsync(quit: true, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Email sent to {Recipient} via SMTP", message.Recipient.Email);

            return NotificationResult.Success(notificationId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send email notification {NotificationId}", notificationId);
            return NotificationResult.Failure(notificationId, "Email delivery failed.");
        }
    }

    private MimeMessage BuildMimeMessage(NotificationMessage message)
    {
        var recipientEmail = message.Recipient.Email;
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            throw new InvalidOperationException("Recipient email address is required for email notifications.");
        }

        var mimeMessage = new MimeMessage();

        mimeMessage.From.Add(new MailboxAddress(_options.SenderName ?? _options.SenderEmail, _options.SenderEmail));
        mimeMessage.To.Add(new MailboxAddress(message.Recipient.Name ?? recipientEmail, recipientEmail));
        mimeMessage.Subject = message.Content.Subject ?? string.Empty;

        var builder = new BodyBuilder();

        if (message.Content.HtmlBody is not null)
        {
            builder.HtmlBody = message.Content.HtmlBody;
        }

        if (message.Content.PlainTextBody is not null)
        {
            builder.TextBody = message.Content.PlainTextBody;
        }

        mimeMessage.Body = builder.ToMessageBody();

        return mimeMessage;
    }
}
