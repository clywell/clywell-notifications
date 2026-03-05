using Microsoft.Extensions.Logging.Abstractions;

namespace Clywell.Core.Notifications.Sse.Tests;

public sealed class SseNotificationChannelTests
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
        var channel = CreateChannel();
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
    public async Task SendAsync_UserBased_WithActiveConnections_WritesToAll()
    {
        var writtenData = new List<string>();
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });
        manager.AddConnection("conn-2", "user-1", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });

        var channel = CreateChannel(connectionManager: manager);
        var message = CreateMessage(userId: "user-1", connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.Equal(2, writtenData.Count);
        Assert.All(writtenData, data => Assert.Contains("event: notification", data));
    }

    [Fact]
    public async Task SendAsync_UserBased_NoActiveConnections_ReturnsFailedResult()
    {
        var manager = new SseConnectionManager();
        var channel = CreateChannel(connectionManager: manager);
        var message = CreateMessage(userId: "user-1", connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("No active SSE connections", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_ConnectionBased_WithActiveConnection_WritesToConnection()
    {
        var writtenData = new List<string>();
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });

        var channel = CreateChannel(options => options.UseConnectionAddressing(), connectionManager: manager);
        var message = CreateMessage(userId: null, connectionId: "conn-1");

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.Single(writtenData);
    }

    [Fact]
    public async Task SendAsync_ConnectionBased_NoActiveConnection_ReturnsFailedResult()
    {
        var manager = new SseConnectionManager();
        var channel = CreateChannel(options => options.UseConnectionAddressing(), connectionManager: manager);
        var message = CreateMessage(userId: null, connectionId: "nonexistent");

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("No active SSE connection", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_WhenWriterThrows_ReturnsFailedResult()
    {
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (_, _) => throw new InvalidOperationException("stream closed"));

        var channel = CreateChannel(connectionManager: manager);
        var message = CreateMessage(userId: "user-1", connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("All SSE connections failed", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (_, _) => throw new OperationCanceledException());

        var channel = CreateChannel(connectionManager: manager);
        var message = CreateMessage(userId: "user-1", connectionId: null);

        await Assert.ThrowsAsync<OperationCanceledException>(() => channel.SendAsync(message));
    }

    [Fact]
    public async Task SendAsync_UserBased_ContinuesOnIndividualWriterFailure()
    {
        var writtenData = new List<string>();
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (_, _) => throw new InvalidOperationException("stream closed"));
        manager.AddConnection("conn-2", "user-1", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });

        var channel = CreateChannel(connectionManager: manager);
        var message = CreateMessage(userId: "user-1", connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.Single(writtenData);
    }

    [Fact]
    public async Task SendAsync_UserBased_AllWritersFail_ReturnsFailedResult()
    {
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (_, _) => throw new InvalidOperationException("closed"));
        manager.AddConnection("conn-2", "user-1", (_, _) => throw new InvalidOperationException("closed"));

        var channel = CreateChannel(connectionManager: manager);
        var message = CreateMessage(userId: "user-1", connectionId: null);

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("All SSE connections failed", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_CustomEventName_IncludesInOutput()
    {
        var writtenData = new List<string>();
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });

        var channel = CreateChannel(options => options.WithEventName("custom-event"), connectionManager: manager);
        var message = CreateMessage(userId: "user-1", connectionId: null);

        await channel.SendAsync(message);

        Assert.Single(writtenData);
        Assert.Contains("event: custom-event", writtenData[0]);
    }

    [Fact]
    public async Task SendAsync_GroupBased_WithActiveConnections_WritesToAllGroupConnections()
    {
        var writtenData = new List<string>();
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });
        manager.AddConnection("conn-2", "user-2", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });
        manager.AddConnectionToGroup("conn-1", "admins");
        manager.AddConnectionToGroup("conn-2", "admins");

        var channel = CreateChannel(connectionManager: manager);
        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { Groups = ["admins"] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.Equal(2, writtenData.Count);
    }

    [Fact]
    public async Task SendAsync_GroupBased_MultipleGroups_WritesToAllGroups()
    {
        var writtenData = new List<string>();
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });
        manager.AddConnection("conn-2", "user-2", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });
        manager.AddConnectionToGroup("conn-1", "admins");
        manager.AddConnectionToGroup("conn-2", "managers");

        var channel = CreateChannel(connectionManager: manager);
        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { Groups = ["admins", "managers"] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.Equal(2, writtenData.Count);
    }

    [Fact]
    public async Task SendAsync_GroupBased_NoConnectionsInGroup_ReturnsFailedResult()
    {
        var manager = new SseConnectionManager();
        var channel = CreateChannel(connectionManager: manager);
        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { Groups = ["admins"] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("all groups", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendAsync_GroupBased_EmptyGroups_FallsThroughToUserAddressing()
    {
        var writtenData = new List<string>();
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (data, _) => { writtenData.Add(data); return Task.CompletedTask; });

        var channel = CreateChannel(connectionManager: manager);
        // Groups is empty - falls through to user-based addressing
        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient { UserId = "user-1", Groups = [] },
            Content = new RenderedContent("Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal
        };

        var result = await channel.SendAsync(message);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.Single(writtenData);
    }

    private static SseNotificationChannel CreateChannel(
        Action<SseOptions>? options = null,
        ISseConnectionManager? connectionManager = null)
    {
        var sseOptions = new SseOptions();
        options?.Invoke(sseOptions);
        return new SseNotificationChannel(
            connectionManager ?? new SseConnectionManager(),
            sseOptions,
            NullLogger<SseNotificationChannel>.Instance);
    }

    private static NotificationMessage CreateMessage(string? userId, string? connectionId)
        => new()
        {
            Recipient = new NotificationRecipient { UserId = userId, ConnectionId = connectionId, Name = "Test User" },
            Content = new RenderedContent("Notification Subject", "<p>Body</p>", "Body"),
            Priority = NotificationPriority.Normal,
        };
}
