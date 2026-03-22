namespace Clywell.Core.Notifications;

/// <summary>
/// Controls how template rendering is handled by <see cref="NotificationService"/>.
/// </summary>
public enum RenderingMode
{
    /// <summary>
    /// Templates are rendered locally by a registered <see cref="ITemplateRenderer"/>.
    /// An <see cref="InvalidOperationException"/> is thrown at dispatch time if a
    /// <see cref="NotificationRequest.TemplateKey"/> is set but no renderer is registered.
    /// </summary>
    Local,

    /// <summary>
    /// Template rendering is delegated to the channel or an external system.
    /// <see cref="NotificationService"/> skips local rendering entirely and passes
    /// <see cref="NotificationMessage.TemplateKey"/> and <see cref="NotificationMessage.Parameters"/>
    /// through to the channel for it to handle.
    /// </summary>
    Delegated
}