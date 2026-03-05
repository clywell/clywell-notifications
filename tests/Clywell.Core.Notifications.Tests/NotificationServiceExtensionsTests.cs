namespace Clywell.Core.Notifications.Tests;

public sealed class NotificationServiceExtensionsTests
{
    [Fact]
    public async Task SendEmailAsync_DispatchesEmailRequest()
    {
        var service = new Mock<INotificationService>();
        var successResult = CreateSuccessResult();
        service
            .Setup(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        await service.Object.SendEmailAsync(b => b.To("test@example.com"));

        service.Verify(
            x => x.SendAsync(
                It.Is<NotificationRequest>(r =>
                    r.Channel == NotificationChannel.Email &&
                    r.Recipient.Email == "test@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendSmsAsync_DispatchesSmsRequest()
    {
        var service = new Mock<INotificationService>();
        var successResult = CreateSuccessResult();
        service
            .Setup(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        await service.Object.SendSmsAsync(b => b.To("+1234567890"));

        service.Verify(
            x => x.SendAsync(
                It.Is<NotificationRequest>(r =>
                    r.Channel == NotificationChannel.Sms &&
                    r.Recipient.PhoneNumber == "+1234567890"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPushAsync_DispatchesPushRequest()
    {
        var service = new Mock<INotificationService>();
        var successResult = CreateSuccessResult();
        service
            .Setup(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        await service.Object.SendPushAsync(b => b.ToUser("push-user"));

        service.Verify(
            x => x.SendAsync(
                It.Is<NotificationRequest>(r =>
                    r.Channel == NotificationChannel.Push &&
                    r.Recipient.UserId == "push-user"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendInAppAsync_DispatchesInAppRequest()
    {
        var service = new Mock<INotificationService>();
        var successResult = CreateSuccessResult();
        service
            .Setup(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        await service.Object.SendInAppAsync(b => b.ToUser("inapp-user"));

        service.Verify(
            x => x.SendAsync(
                It.Is<NotificationRequest>(r =>
                    r.Channel == NotificationChannel.InApp &&
                    r.Recipient.UserId == "inapp-user"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_GenericOverload_ViaEmail_DispatchesEmailRequest()
    {
        var service = new Mock<INotificationService>();
        var successResult = CreateSuccessResult();
        service
            .Setup(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        await service.Object.SendAsync(() => NotificationBuilder.ViaEmail().To("a@b.com"));

        service.Verify(
            x => x.SendAsync(
                It.Is<NotificationRequest>(r => r.Channel == NotificationChannel.Email),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_GenericOverload_ViaInApp_DispatchesInAppRequest()
    {
        var service = new Mock<INotificationService>();
        var successResult = CreateSuccessResult();
        service
            .Setup(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        await service.Object.SendAsync(() => NotificationBuilder.ViaInApp().ToUser("u"));

        service.Verify(
            x => x.SendAsync(
                It.Is<NotificationRequest>(r => r.Channel == NotificationChannel.InApp),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_ForwardsCancellationToken()
    {
        var service = new Mock<INotificationService>();
        var successResult = CreateSuccessResult();
        var cancellationToken = new CancellationTokenSource().Token;

        service
            .Setup(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        await service.Object.SendEmailAsync(b => b.To("test@example.com"), cancellationToken);

        service.Verify(
            x => x.SendAsync(
                It.IsAny<NotificationRequest>(),
                It.Is<CancellationToken>(ct => ct == cancellationToken)),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenBuilderThrows_PropagatesBeforeCallingService()
    {
        var service = new Mock<INotificationService>();
        var successResult = CreateSuccessResult();

        service
            .Setup(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Object.SendEmailAsync(_ => { }));

        service.Verify(x => x.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_NullService_ThrowsArgumentNullException()
    {
        INotificationService nullService = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => nullService.SendEmailAsync(_ => { }));
    }

    [Fact]
    public async Task SendEmailAsync_NullConfigure_ThrowsArgumentNullException()
    {
        var service = new Mock<INotificationService>();
        Action<EmailNotificationBuilder> configure = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.Object.SendEmailAsync(configure));
    }

    private static NotificationResult CreateSuccessResult()
    {
        return new NotificationResult
        {
            NotificationId = "test-id",
            Status = NotificationStatus.Sent,
            SentAt = DateTimeOffset.UtcNow
        };
    }
}
