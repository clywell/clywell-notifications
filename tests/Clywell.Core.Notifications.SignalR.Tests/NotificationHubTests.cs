using System.Reflection;
using Microsoft.AspNetCore.SignalR;

namespace Clywell.Core.Notifications.SignalR.Tests;

public sealed class NotificationHubTests
{
    [Fact]
    public async Task JoinGroupAsync_WithMatchingUserIdentifier_Succeeds()
    {
        var hub = CreateHub("user-123");

        await hub.JoinGroupAsync("user-123");

        Mock.Get(hub.Groups).Verify(g => g.AddToGroupAsync(
            It.IsAny<string>(), "user-123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task JoinGroupAsync_WithDifferentUserIdentifier_ThrowsHubException()
    {
        var hub = CreateHub("user-123");

        await Assert.ThrowsAsync<HubException>(() => hub.JoinGroupAsync("other-user"));
    }

    [Fact]
    public async Task LeaveGroupAsync_WithMatchingUserIdentifier_Succeeds()
    {
        var hub = CreateHub("user-123");

        await hub.LeaveGroupAsync("user-123");

        Mock.Get(hub.Groups).Verify(g => g.RemoveFromGroupAsync(
            It.IsAny<string>(), "user-123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LeaveGroupAsync_WithDifferentUserIdentifier_ThrowsHubException()
    {
        var hub = CreateHub("user-123");

        await Assert.ThrowsAsync<HubException>(() => hub.LeaveGroupAsync("other-user"));
    }

    [Fact]
    public async Task JoinGroupAsync_WithNullOrWhitespace_ThrowsArgumentException()
    {
        var hub = CreateHub("user-123");

        await Assert.ThrowsAsync<ArgumentException>(() => hub.JoinGroupAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() => hub.JoinGroupAsync("  "));
    }

    private static NotificationHub CreateHub(string userIdentifier)
    {
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.UserIdentifier).Returns(userIdentifier);
        mockContext.Setup(c => c.ConnectionId).Returns("conn-" + Guid.NewGuid().ToString("N"));

        var mockGroups = new Mock<IGroupManager>();
        mockGroups
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockGroups
            .Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var hub = new NotificationHub();
        SetHubProperty(hub, "Context", mockContext.Object);
        SetHubProperty(hub, "Groups", mockGroups.Object);

        return hub;
    }

    private static void SetHubProperty(Hub hub, string propertyName, object value)
    {
        var property = typeof(Hub).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property?.SetValue(hub, value);
    }
}
