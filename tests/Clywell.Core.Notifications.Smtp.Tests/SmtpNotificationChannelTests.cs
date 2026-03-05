using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging.Abstractions;
using MimeKit;

namespace Clywell.Core.Notifications.Smtp.Tests;

public sealed class SmtpNotificationChannelTests
{
    [Fact]
    public void Channel_ReturnsEmail()
    {
        var channel = CreateChannel();

        var result = channel.Channel;

        Assert.Equal(NotificationChannel.Email, result);
    }

    [Fact]
    public async Task SendAsync_WithNullRecipientEmail_ReturnsFailedResult()
    {
        var smtpClientFactory = new Mock<ISmtpClientFactory>(MockBehavior.Strict);
        var channel = CreateChannel(smtpClientFactory: smtpClientFactory.Object);
        var message = CreateMessage(email: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Equal("Recipient email address is required for email notifications.", result.ErrorMessage);
        smtpClientFactory.Verify(x => x.Create(), Times.Never);
    }

    [Fact]
    public async Task SendAsync_WithEmptyRecipientEmail_ReturnsFailedResult()
    {
        var smtpClientFactory = new Mock<ISmtpClientFactory>(MockBehavior.Strict);
        var channel = CreateChannel(smtpClientFactory: smtpClientFactory.Object);
        var message = CreateMessage(email: string.Empty);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Equal("Recipient email address is required for email notifications.", result.ErrorMessage);
        smtpClientFactory.Verify(x => x.Create(), Times.Never);
    }

    [Fact]
    public async Task SendAsync_WithValidMessage_SendsEmailUsingSmtpClient()
    {
        var mimeMessageSent = default(MimeMessage);
        var smtpClient = new Mock<ISmtpClient>(MockBehavior.Strict);
        smtpClient
            .Setup(x => x.ConnectAsync("smtp.example.com", 587, SecureSocketOptions.StartTls, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        smtpClient
            .Setup(x => x.AuthenticateAsync("smtp-user", "smtp-pass", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        smtpClient
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Callback<MimeMessage, CancellationToken, ITransferProgress?>((message, _, _) => mimeMessageSent = message)
            .Returns(Task.FromResult("message-id"));
        smtpClient
            .Setup(x => x.DisconnectAsync(true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        smtpClient.Setup(x => x.Dispose());

        var smtpClientFactory = new Mock<ISmtpClientFactory>(MockBehavior.Strict);
        smtpClientFactory
            .Setup(x => x.Create())
            .Returns(smtpClient.Object);

        var channel = CreateChannel(CreateOptionsWithCredentials(), smtpClientFactory.Object);
        var message = CreateMessage("recipient@example.com");

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.Null(result.ErrorMessage);

        smtpClientFactory.Verify(x => x.Create(), Times.Once);
        smtpClient.Verify(x => x.ConnectAsync("smtp.example.com", 587, SecureSocketOptions.StartTls, It.IsAny<CancellationToken>()), Times.Once);
        smtpClient.Verify(x => x.AuthenticateAsync("smtp-user", "smtp-pass", It.IsAny<CancellationToken>()), Times.Once);
        smtpClient.Verify(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()), Times.Once);
        smtpClient.Verify(x => x.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
        smtpClient.Verify(x => x.Dispose(), Times.Once);

        Assert.NotNull(mimeMessageSent);
        Assert.Equal("noreply@example.com", mimeMessageSent.From.Mailboxes.Single().Address);
        Assert.Equal("No Reply", mimeMessageSent.From.Mailboxes.Single().Name);
        Assert.Equal("recipient@example.com", mimeMessageSent.To.Mailboxes.Single().Address);
        Assert.Equal("Test User", mimeMessageSent.To.Mailboxes.Single().Name);
        Assert.Equal("Subject", mimeMessageSent.Subject);
        Assert.Equal("<p>Body</p>", mimeMessageSent.HtmlBody);
        Assert.Equal("Body", mimeMessageSent.TextBody);
    }

    [Fact]
    public async Task SendAsync_WhenSmtpThrows_ReturnsGenericFailureMessage()
    {
        var smtpClient = new Mock<ISmtpClient>(MockBehavior.Strict);
        smtpClient
            .Setup(x => x.ConnectAsync("smtp.example.com", 587, SecureSocketOptions.StartTls, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp.example.com authentication failed"));
        smtpClient.Setup(x => x.Dispose());

        var smtpClientFactory = new Mock<ISmtpClientFactory>(MockBehavior.Strict);
        smtpClientFactory
            .Setup(x => x.Create())
            .Returns(smtpClient.Object);

        var channel = CreateChannel(CreateOptionsWithCredentials(), smtpClientFactory.Object);
        var message = CreateMessage("recipient@example.com");

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Equal("Email delivery failed.", result.ErrorMessage);

        smtpClientFactory.Verify(x => x.Create(), Times.Once);
        smtpClient.Verify(x => x.ConnectAsync("smtp.example.com", 587, SecureSocketOptions.StartTls, It.IsAny<CancellationToken>()), Times.Once);
        smtpClient.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void SmtpOptions_Defaults_PortIs587()
    {
        var options = new SmtpOptions();

        Assert.Equal(587, options.Port);
    }

    [Fact]
    public void SmtpOptions_Defaults_UseSslIsTrue()
    {
        var options = new SmtpOptions();

        Assert.True(options.UseSsl);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        var smtpClientFactory = new Mock<ISmtpClientFactory>(MockBehavior.Strict).Object;
        var logger = NullLogger<SmtpNotificationChannel>.Instance;

        Assert.Throws<ArgumentNullException>(() => new SmtpNotificationChannel(null!, smtpClientFactory, logger));
    }

    [Fact]
    public void Constructor_WithNullSmtpClientFactory_ThrowsArgumentNullException()
    {
        var options = CreateOptions();
        var logger = NullLogger<SmtpNotificationChannel>.Instance;

        Assert.Throws<ArgumentNullException>(() => new SmtpNotificationChannel(options, null!, logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var options = CreateOptions();
        var smtpClientFactory = new Mock<ISmtpClientFactory>(MockBehavior.Strict).Object;

        Assert.Throws<ArgumentNullException>(() => new SmtpNotificationChannel(options, smtpClientFactory, null!));
    }

    private static SmtpNotificationChannel CreateChannel(
        SmtpOptions? options = null,
        ISmtpClientFactory? smtpClientFactory = null)
    {
        var clientFactory = smtpClientFactory ?? new Mock<ISmtpClientFactory>(MockBehavior.Strict).Object;
        return new SmtpNotificationChannel(options ?? CreateOptions(), clientFactory, NullLogger<SmtpNotificationChannel>.Instance);
    }

    private static SmtpOptions CreateOptions()
        => new SmtpOptions()
            .UseHost("smtp.example.com")
            .UseSender("noreply@example.com");

    private static SmtpOptions CreateOptionsWithCredentials()
        => new SmtpOptions()
            .UseHost("smtp.example.com")
            .WithCredentials("smtp-user", "smtp-pass")
            .WithSsl()
            .UseSender("noreply@example.com", "No Reply");

    private static NotificationMessage CreateMessage(string? email)
        => new()
        {
            Recipient = new NotificationRecipient { Email = email, Name = "Test User" },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal,
        };
}
