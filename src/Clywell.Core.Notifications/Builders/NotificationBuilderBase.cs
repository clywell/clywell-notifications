namespace Clywell.Core.Notifications;

/// <summary>
/// Provides common fluent configuration behavior for channel-specific notification builders.
/// </summary>
/// <typeparam name="TSelf">The concrete builder type.</typeparam>
/// <remarks>
/// Initializes a new builder for a specific notification channel.
/// </remarks>
/// <param name="channel">The notification channel.</param>
public abstract class NotificationBuilderBase<TSelf>(NotificationChannel channel) : INotificationBuilder
    where TSelf : NotificationBuilderBase<TSelf>
{
    /// <summary>
    /// The selected channel for the notification.
    /// </summary>
    protected readonly NotificationChannel _channel = channel;

    /// <summary>
    /// The recipient for the notification.
    /// </summary>
    protected NotificationRecipient? _recipient;

    /// <summary>
    /// The template key used for template rendering.
    /// </summary>
    protected string? _templateKey;

    /// <summary>
    /// The notification subject or title.
    /// </summary>
    protected string? _subject;

    /// <summary>
    /// The notification body.
    /// </summary>
    protected string? _body;

    /// <summary>
    /// Template parameters for rendering.
    /// </summary>
    protected readonly Dictionary<string, object> _parameters = [];

    /// <summary>
    /// Delivery priority.
    /// </summary>
    protected NotificationPriority _priority = NotificationPriority.Normal;

    /// <summary>
    /// Custom metadata values.
    /// </summary>
    protected readonly Dictionary<string, string> _metadata = [];

    /// <summary>
    /// Sets the template key for template-based rendering.
    /// </summary>
    /// <param name="templateKey">The template key.</param>
    /// <returns>The current builder instance.</returns>
    public TSelf WithTemplate(string templateKey)
    {
        _templateKey = templateKey;
        return (TSelf)this;
    }

    /// <summary>
    /// Sets the notification body.
    /// </summary>
    /// <param name="body">The notification body.</param>
    /// <returns>The current builder instance.</returns>
    public TSelf WithBody(string body)
    {
        _body = body;
        return (TSelf)this;
    }

    /// <summary>
    /// Adds or replaces a template parameter.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The current builder instance.</returns>
    public TSelf WithParameter(string key, object value)
    {
        _parameters[key] = value;
        return (TSelf)this;
    }

    /// <summary>
    /// Adds or replaces multiple template parameters.
    /// </summary>
    /// <param name="parameters">The parameters to merge.</param>
    /// <returns>The current builder instance.</returns>
    public TSelf WithParameters(IEnumerable<KeyValuePair<string, object>> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        foreach (var (key, value) in parameters)
        {
            _parameters[key] = value;
        }

        return (TSelf)this;
    }

    /// <summary>
    /// Sets the notification priority.
    /// </summary>
    /// <param name="priority">The priority value.</param>
    /// <returns>The current builder instance.</returns>
    public TSelf WithPriority(NotificationPriority priority)
    {
        _priority = priority;
        return (TSelf)this;
    }

    /// <summary>
    /// Adds or replaces a metadata entry.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current builder instance.</returns>
    public TSelf WithMetadata(string key, string value)
    {
        _metadata[key] = value;
        return (TSelf)this;
    }

    /// <summary>
    /// Builds a notification request from the configured values.
    /// </summary>
    /// <returns>The built notification request.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no recipient has been configured.</exception>
    public NotificationRequest Build()
    {
        if (_recipient is null)
        {
            throw new InvalidOperationException("Recipient must be set before building.");
        }

        return new NotificationRequest
        {
            Channel = _channel,
            Recipient = _recipient,
            TemplateKey = _templateKey,
            Subject = _subject,
            Body = _body,
            Parameters = new Dictionary<string, object>(_parameters),
            Priority = _priority,
            Metadata = new Dictionary<string, string>(_metadata)
        };
    }
}
