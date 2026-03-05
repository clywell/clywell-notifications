namespace Clywell.Core.Notifications.SignalR;

/// <summary>
/// Configuration options for the SignalR notification channel.
/// </summary>
public sealed class SignalROptions
{
    /// <summary>The client method name invoked when sending notifications. Defaults to "ReceiveNotification".</summary>
    public string MethodName { get; private set; } = "ReceiveNotification";

    /// <summary>
    /// When true, targets the user via their UserId using SignalR's user-based addressing.
    /// When false, targets by ConnectionId or group. Defaults to true.
    /// </summary>
    public bool UseUserBasedAddressing { get; private set; } = true;

    /// <summary>Sets the client method name invoked for notifications.</summary>
    public SignalROptions WithMethodName(string methodName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);
        MethodName = methodName;
        return this;
    }

    /// <summary>Configures targeting to use SignalR user-based addressing (via UserId).</summary>
    public SignalROptions UseUserAddressing()
    {
        UseUserBasedAddressing = true;
        return this;
    }

    /// <summary>Configures targeting to use direct connection addressing (via ConnectionId).</summary>
    public SignalROptions UseConnectionAddressing()
    {
        UseUserBasedAddressing = false;
        return this;
    }
}
