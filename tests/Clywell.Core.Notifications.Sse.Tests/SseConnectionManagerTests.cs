namespace Clywell.Core.Notifications.Sse.Tests;

public sealed class SseConnectionManagerTests
{
    [Fact]
    public void AddConnection_AndGetById_ReturnsWriter()
    {
        var manager = new SseConnectionManager();
        Func<string, CancellationToken, Task> writer = (_, _) => Task.CompletedTask;

        manager.AddConnection("conn-1", "user-1", writer);

        var result = manager.GetConnectionById("conn-1");
        Assert.Same(writer, result);
    }

    [Fact]
    public void GetConnectionById_NotFound_ReturnsNull()
    {
        var manager = new SseConnectionManager();

        var result = manager.GetConnectionById("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void GetConnectionsByUserId_ReturnsAllForUser()
    {
        var manager = new SseConnectionManager();
        Func<string, CancellationToken, Task> writer1 = (_, _) => Task.CompletedTask;
        Func<string, CancellationToken, Task> writer2 = (_, _) => Task.CompletedTask;

        manager.AddConnection("conn-1", "user-1", writer1);
        manager.AddConnection("conn-2", "user-1", writer2);
        manager.AddConnection("conn-3", "user-2", (_, _) => Task.CompletedTask);

        var result = manager.GetConnectionsByUserId("user-1");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void RemoveConnection_RemovesFromManager()
    {
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (_, _) => Task.CompletedTask);

        manager.RemoveConnection("conn-1");

        Assert.Null(manager.GetConnectionById("conn-1"));
        Assert.Empty(manager.GetConnectionsByUserId("user-1"));
    }

    [Fact]
    public void RemoveConnection_Nonexistent_DoesNotThrow()
    {
        var manager = new SseConnectionManager();

        var exception = Record.Exception(() => manager.RemoveConnection("nonexistent"));

        Assert.Null(exception);
    }

    [Fact]
    public void GetConnectionsByUserId_NoConnections_ReturnsEmptyList()
    {
        var manager = new SseConnectionManager();

        var result = manager.GetConnectionsByUserId("user-1");

        Assert.Empty(result);
    }

    [Fact]
    public void AddConnectionToGroup_ThenGetConnectionsByGroup_ReturnsWriter()
    {
        var manager = new SseConnectionManager();
        var written = new List<string>();
        manager.AddConnection("conn-1", "user-1", (data, _) => { written.Add(data); return Task.CompletedTask; });
        manager.AddConnectionToGroup("conn-1", "admins");

        var writers = manager.GetConnectionsByGroup("admins");

        Assert.Single(writers);
    }

    [Fact]
    public void GetConnectionsByGroup_WithNoConnections_ReturnsEmpty()
    {
        var manager = new SseConnectionManager();

        var writers = manager.GetConnectionsByGroup("admins");

        Assert.Empty(writers);
    }

    [Fact]
    public void RemoveConnectionFromGroup_RemovesConnectionFromGroup()
    {
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (_, _) => Task.CompletedTask);
        manager.AddConnectionToGroup("conn-1", "admins");
        manager.RemoveConnectionFromGroup("conn-1", "admins");

        var writers = manager.GetConnectionsByGroup("admins");

        Assert.Empty(writers);
    }

    [Fact]
    public void GetConnectionsByGroup_MultipleConnectionsInGroup_ReturnsAll()
    {
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (_, _) => Task.CompletedTask);
        manager.AddConnection("conn-2", "user-2", (_, _) => Task.CompletedTask);
        manager.AddConnectionToGroup("conn-1", "admins");
        manager.AddConnectionToGroup("conn-2", "admins");

        var writers = manager.GetConnectionsByGroup("admins");

        Assert.Equal(2, writers.Count);
    }

    [Fact]
    public void GetConnectionsByGroup_AfterConnectionRemoved_DoesNotReturnStaleWriter()
    {
        // Connection removed via RemoveConnection (not RemoveConnectionFromGroup)
        // The group still has the connectionId but the connection no longer exists in _connections
        // GetConnectionsByGroup should skip missing connections
        var manager = new SseConnectionManager();
        manager.AddConnection("conn-1", "user-1", (_, _) => Task.CompletedTask);
        manager.AddConnectionToGroup("conn-1", "admins");
        manager.RemoveConnection("conn-1"); // connection gone but group still has it

        var writers = manager.GetConnectionsByGroup("admins");

        Assert.Empty(writers); // stale entry silently skipped
    }

    [Fact]
    public void AddConnectionToGroup_WithNullOrWhitespaceConnectionId_Throws()
    {
        var manager = new SseConnectionManager();
        Assert.Throws<ArgumentException>(() => manager.AddConnectionToGroup("  ", "admins"));
    }

    [Fact]
    public void AddConnectionToGroup_WithNullOrWhitespaceGroup_Throws()
    {
        var manager = new SseConnectionManager();
        Assert.Throws<ArgumentException>(() => manager.AddConnectionToGroup("conn-1", "  "));
    }
}
