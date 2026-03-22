namespace Clywell.Core.Notifications;

/// <summary>
/// Message representing fully resolved notification content passed to channel implementations.
/// </summary>
public sealed record NotificationMessage
{
    /// <summary>Gets the notification recipient.</summary>
    public required NotificationRecipient Recipient { get; init; }

    /// <summary>Gets the rendered content.</summary>
    public required RenderedContent Content { get; init; }

    /// <summary>Gets the delivery priority.</summary>
    public required NotificationPriority Priority { get; init; }

    /// <summary>Gets custom metadata for tracking.</summary>
    public Dictionary<string, string> Metadata { get; init; } = [];

    /// <summary>Gets the template key, if the notification was template-based.</summary>
    public string? TemplateKey { get; init; }

    /// <summary>Gets the raw template parameters, if the notification was template-based.</summary>
    public IReadOnlyDictionary<string, object>? Parameters { get; init; }
}