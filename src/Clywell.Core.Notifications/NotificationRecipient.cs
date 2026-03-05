namespace Clywell.Core.Notifications;

/// <summary>
/// Represents the target recipient of a notification.
/// </summary>
public sealed record NotificationRecipient
{
    /// <summary>Email address for the Email channel.</summary>
    public string? Email { get; init; }

    /// <summary>Phone number for the SMS channel.</summary>
    public string? PhoneNumber { get; init; }

    /// <summary>Application user identifier for the InApp and Push channels.</summary>
    public string? UserId { get; init; }

    /// <summary>Real-time connection identifier for targeted SignalR/SSE delivery.</summary>
    public string? ConnectionId { get; init; }

    /// <summary>Device token for push notification services.</summary>
    public string? DeviceToken { get; init; }

    /// <summary>Display name of the recipient.</summary>
    public string? Name { get; init; }

    /// <summary>One or more group identifiers (e.g. roles, tenants) for group-based in-app delivery. When set, the channel will dispatch to each group sequentially.</summary>
    public IReadOnlyList<string> Groups { get; init; } = [];
}