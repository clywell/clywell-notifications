namespace Clywell.Core.Notifications;

/// <summary>
/// Configuration options for the notification system.
/// </summary>
public sealed class NotificationOptions
{
    /// <summary>The default channel used when no channel is specified on a request.</summary>
    public NotificationChannel DefaultChannel { get; private set; } = NotificationChannel.Email;

    /// <summary>Gets the rendering mode. Defaults to <see cref="RenderingMode.Local"/>.</summary>
    public RenderingMode RenderingMode { get; private set; } = RenderingMode.Local;

    /// <summary>Maximum number of retry attempts for failed notifications.</summary>
    public int MaxRetryAttempts { get; private set; } = 3;

    /// <summary>Delay between retry attempts.</summary>
    public TimeSpan RetryDelay { get; private set; } = TimeSpan.FromSeconds(2);

    /// <summary>Sets the default notification channel.</summary>
    public NotificationOptions UseDefaultChannel(NotificationChannel channel)
    {
        DefaultChannel = channel;
        return this;
    }

    /// <summary>Sets the maximum retry attempts for failed notifications.</summary>
    public NotificationOptions WithMaxRetryAttempts(int attempts)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(attempts);
        MaxRetryAttempts = attempts;
        return this;
    }

    /// <summary>Sets the delay between retry attempts.</summary>
    public NotificationOptions WithRetryDelay(TimeSpan delay)
    {
        RetryDelay = delay;
        return this;
    }

    /// <summary>
    /// Configures the rendering mode.
    /// Use <see cref="RenderingMode.Delegated"/> when channels handle rendering themselves
    /// (e.g. by forwarding to an external API) and no local <see cref="ITemplateRenderer"/> is needed.
    /// </summary>
    public NotificationOptions WithRenderingMode(RenderingMode mode)
    {
        RenderingMode = mode;
        return this;
    }
}