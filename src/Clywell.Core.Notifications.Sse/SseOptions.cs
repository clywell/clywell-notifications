namespace Clywell.Core.Notifications.Sse;

/// <summary>
/// Configuration options for the SSE notification channel.
/// </summary>
public sealed class SseOptions
{
    /// <summary>The SSE event name sent to clients. Defaults to "notification".</summary>
    public string EventName { get; private set; } = "notification";

    /// <summary>
    /// When true, targets by UserId (sends to all connections for that user).
    /// When false, targets by ConnectionId (single connection). Defaults to true.
    /// </summary>
    public bool UseUserBasedAddressing { get; private set; } = true;

    /// <summary>Sets the SSE event name.</summary>
    public SseOptions WithEventName(string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        EventName = eventName;
        return this;
    }

    /// <summary>Configures targeting to use user-based addressing (all connections for a UserId).</summary>
    public SseOptions UseUserAddressing()
    {
        UseUserBasedAddressing = true;
        return this;
    }

    /// <summary>Configures targeting to use direct connection addressing (single ConnectionId).</summary>
    public SseOptions UseConnectionAddressing()
    {
        UseUserBasedAddressing = false;
        return this;
    }
}
