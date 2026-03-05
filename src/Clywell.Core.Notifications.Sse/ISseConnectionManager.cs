namespace Clywell.Core.Notifications.Sse;

/// <summary>
/// Manages active SSE connections, mapping user IDs and connection IDs to response streams.
/// </summary>
public interface ISseConnectionManager
{
    /// <summary>
    /// Registers a new SSE client connection.
    /// </summary>
    /// <param name="connectionId">Unique connection identifier.</param>
    /// <param name="userId">The user ID associated with this connection.</param>
    /// <param name="writer">A delegate that writes SSE data to the client's response stream.</param>
    void AddConnection(string connectionId, string userId, Func<string, CancellationToken, Task> writer);

    /// <summary>
    /// Removes an SSE client connection.
    /// </summary>
    void RemoveConnection(string connectionId);

    /// <summary>
    /// Gets all active writer delegates for a given user ID.
    /// </summary>
    IReadOnlyList<Func<string, CancellationToken, Task>> GetConnectionsByUserId(string userId);

    /// <summary>
    /// Gets the writer delegate for a specific connection ID, or null if not found.
    /// </summary>
    Func<string, CancellationToken, Task>? GetConnectionById(string connectionId);

    /// <summary>
    /// Associates a connection with a named group (e.g. a role or tenant identifier).
    /// </summary>
    /// <param name="connectionId">The connection to add to the group.</param>
    /// <param name="group">The group name.</param>
    void AddConnectionToGroup(string connectionId, string group);

    /// <summary>
    /// Removes a connection from a named group.
    /// </summary>
    /// <param name="connectionId">The connection to remove from the group.</param>
    /// <param name="group">The group name.</param>
    void RemoveConnectionFromGroup(string connectionId, string group);

    /// <summary>
    /// Gets all active writer delegates registered in a named group.
    /// </summary>
    /// <param name="group">The group name.</param>
    IReadOnlyList<Func<string, CancellationToken, Task>> GetConnectionsByGroup(string group);
}
