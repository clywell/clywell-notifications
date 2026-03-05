using System.Collections.Concurrent;

namespace Clywell.Core.Notifications.Sse;

/// <summary>
/// Thread-safe in-memory SSE connection manager.
/// </summary>
internal sealed class SseConnectionManager : ISseConnectionManager
{
    private readonly ConcurrentDictionary<string, SseConnection> _connections = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _groups = new();

    public void AddConnection(string connectionId, string userId, Func<string, CancellationToken, Task> writer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(writer);

        _connections[connectionId] = new SseConnection(userId, writer);
    }

    public void RemoveConnection(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        _connections.TryRemove(connectionId, out _);
    }

    public IReadOnlyList<Func<string, CancellationToken, Task>> GetConnectionsByUserId(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return _connections.Values
            .Where(c => c.UserId == userId)
            .Select(c => c.Writer)
            .ToList();
    }

    public Func<string, CancellationToken, Task>? GetConnectionById(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        return _connections.TryGetValue(connectionId, out var connection) ? connection.Writer : null;
    }

    public void AddConnectionToGroup(string connectionId, string group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        _groups.GetOrAdd(group, _ => new ConcurrentDictionary<string, byte>())
               .TryAdd(connectionId, 0);
    }

    public void RemoveConnectionFromGroup(string connectionId, string group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        if (_groups.TryGetValue(group, out var connections))
        {
            connections.TryRemove(connectionId, out _);
        }
    }

    public IReadOnlyList<Func<string, CancellationToken, Task>> GetConnectionsByGroup(string group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        if (!_groups.TryGetValue(group, out var connectionIds))
        {
            return [];
        }

        var writers = new List<Func<string, CancellationToken, Task>>();
        foreach (var connectionId in connectionIds.Keys)
        {
            if (_connections.TryGetValue(connectionId, out var conn))
            {
                writers.Add(conn.Writer);
            }
        }
        return writers;
    }

    private sealed record SseConnection(string UserId, Func<string, CancellationToken, Task> Writer);
}
