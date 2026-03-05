using Microsoft.AspNetCore.SignalR;

namespace Clywell.Core.Notifications.SignalR;

/// <summary>
/// SignalR hub for real-time notification delivery.
/// Clients connect to this hub and join groups (e.g., by UserId) to receive notifications.
/// </summary>
public sealed class NotificationHub : Hub
{
    /// <summary>
    /// Adds the caller to a notification group (typically their user ID).
    /// </summary>
    public Task JoinGroupAsync(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        if (!string.Equals(groupName, Context.UserIdentifier, StringComparison.Ordinal))
        {
            throw new HubException("Cannot join a group that does not match your user identifier.");
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Removes the caller from a notification group.
    /// </summary>
    public Task LeaveGroupAsync(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        if (!string.Equals(groupName, Context.UserIdentifier, StringComparison.Ordinal))
        {
            throw new HubException("Cannot leave a group that does not match your user identifier.");
        }

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
