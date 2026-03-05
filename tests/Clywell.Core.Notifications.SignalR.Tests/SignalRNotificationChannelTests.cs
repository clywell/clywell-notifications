using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clywell.Core.Notifications.SignalR.Tests;

public sealed class SignalRNotificationChannelTests
{
    [Fact]
    public void Channel_ReturnsInApp()
    {
        var channel = CreateChannel();

        Assert.Equal(NotificationChannel.InApp, channel.Channel);
    }

    [Fact]
    public async Task SendAsync_UserBased_WithNoUserId_ReturnsFailedResult()
    {
        var channel = CreateChannel(options => options.UseUserAddressing());
        var message = CreateMessage(userId: null, connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("UserId is required", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_ConnectionBased_WithNoConnectionId_ReturnsFailedResult()
    {
        var channel = CreateChannel(options => options.UseConnectionAddressing());
        var message = CreateMessage(userId: null, connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("ConnectionId is required", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_UserBased_WithUserId_CallsSendAsyncOnUser()
    {
        var mockClientProxy = new Mock<IClientProxy>();
        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.User("user-123")).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions().UseUserAddressing().WithMethodName("ReceiveNotification");
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);

        var message = CreateMessage(userId: "user-123", connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.NotNull(result.NotificationId);
        mockClients.Verify(x => x.User("user-123"), Times.Once);
        mockClientProxy.Verify(x => x.SendCoreAsync("ReceiveNotification", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ConnectionBased_WithConnectionId_CallsSendAsyncOnClient()
    {
        var mockClientProxy = new Mock<IClientProxy>();
        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.Client("conn-abc")).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions().UseConnectionAddressing();
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);

        var message = CreateMessage(userId: null, connectionId: "conn-abc");

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        mockClients.Verify(x => x.Client("conn-abc"), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenHubThrows_ReturnsFailedResult()
    {
        var mockClientProxy = new Mock<IClientProxy>();
        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("hub disconnected"));

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions();
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);

        var message = CreateMessage(userId: "user-123", connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Equal("Real-time notification delivery failed.", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockClientProxy = new Mock<IClientProxy>();
        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions();
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);
        var message = CreateMessage(userId: "user-123", connectionId: null);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            channel.SendAsync(message, cts.Token));
    }

    [Fact]
    public async Task SendAsync_GroupBased_WithGroups_CallsSendAsyncOnEachGroup()
    {
        var adminsProxy = new Mock<IClientProxy>();
        adminsProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var managersProxy = new Mock<IClientProxy>();
        managersProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.Group("admins")).Returns(adminsProxy.Object);
        mockClients.Setup(x => x.Group("managers")).Returns(managersProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions().UseUserAddressing().WithMethodName("ReceiveNotification");
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);

        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { Groups = ["admins", "managers"] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal,
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        mockClients.Verify(x => x.Group("admins"), Times.Once);
        mockClients.Verify(x => x.Group("managers"), Times.Once);
        adminsProxy.Verify(x => x.SendCoreAsync("ReceiveNotification", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
        managersProxy.Verify(x => x.SendCoreAsync("ReceiveNotification", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_GroupBased_WithEmptyGroups_FallsThroughToUserAddressing()
    {
        var mockClientProxy = new Mock<IClientProxy>();
        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.User("user-1")).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions().UseUserAddressing();
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);

        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { UserId = "user-1", Groups = [] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal,
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        mockClients.Verify(x => x.User("user-1"), Times.Once);
        mockClients.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_GroupBased_AllGroupsFail_ReturnsFailure()
    {
        var failingProxy = new Mock<IClientProxy>();
        failingProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("send failed"));

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(failingProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions().UseUserAddressing();
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);

        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { Groups = ["admins"] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal,
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("all groups", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendAsync_GroupBased_OneGroupFails_ReturnsSuccess()
    {
        var adminsProxy = new Mock<IClientProxy>();
        adminsProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var managersProxy = new Mock<IClientProxy>();
        managersProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("send failed"));

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.Group("admins")).Returns(adminsProxy.Object);
        mockClients.Setup(x => x.Group("managers")).Returns(managersProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions().UseUserAddressing();
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);

        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { Groups = ["admins", "managers"] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal,
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
    }

    [Fact]
    public async Task SendAsync_GroupBased_WithNoGroups_UsesUserAddressingWhenConfigured()
    {
        var mockClientProxy = new Mock<IClientProxy>();
        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(x => x.User("user-1")).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        var options = new SignalROptions().UseUserAddressing();
        var channel = new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);

        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { UserId = "user-1", Groups = [] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal,
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        mockClients.Verify(x => x.User("user-1"), Times.Once);
        mockClients.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
    }

    private static SignalRNotificationChannel CreateChannel(Action<SignalROptions>? configure = null)
    {
        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        var options = new SignalROptions();
        configure?.Invoke(options);
        return new SignalRNotificationChannel(mockHubContext.Object, options, NullLogger<SignalRNotificationChannel>.Instance);
    }

    private static NotificationMessage CreateMessage(string? userId, string? connectionId)
        => new()
        {
            Recipient = new NotificationRecipient { UserId = userId, ConnectionId = connectionId, Name = "Test User" },
            Content = new RenderedContent("Notification Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal,
        };
}
