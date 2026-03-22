namespace Clywell.Core.Notifications;

/// <summary>
/// Represents a request to send a notification.
/// </summary>
public sealed record NotificationRequest
{
    /// <summary>Gets the tenant identifier. Required when routing to a multi-tenant notification service.</summary>
    public Guid? TenantId { get; init; }

    /// <summary>The notification channel. When null, the default from NotificationOptions is used.</summary>
    public NotificationChannel? Channel { get; init; }

    /// <summary>Gets the notification recipient.</summary>
    public required NotificationRecipient Recipient { get; init; }

    /// <summary>Gets the template key for template-based rendering. Mutually optional with inline content.</summary>
    public string? TemplateKey { get; init; }

    /// <summary>Gets the notification subject (for Email). Used as inline content when no template is specified.</summary>
    public string? Subject { get; init; }

    /// <summary>Gets the notification body. Used as inline content when no template is specified.</summary>
    public string? Body { get; init; }

    /// <summary>Gets the template parameters for variable substitution.</summary>
    public Dictionary<string, object> Parameters { get; init; } = [];

    /// <summary>Gets the delivery priority.</summary>
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;

    /// <summary>Gets custom metadata for tracking.</summary>
    public Dictionary<string, string> Metadata { get; init; } = [];
}