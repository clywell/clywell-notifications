namespace Clywell.Core.Notifications;

/// <summary>
/// Default implementation of <see cref="INotificationService"/>. Resolves the correct
/// <see cref="INotificationChannel"/> for each request, renders templates if needed,
/// and dispatches for delivery.
/// </summary>
internal sealed class NotificationService : INotificationService
{
    private readonly Dictionary<NotificationChannel, INotificationChannel> _channels;
    private readonly NotificationOptions _options;
    private readonly ITemplateRenderer? _templateRenderer;
    private readonly INotificationLogger? _notificationLogger;
    private readonly ILogger<NotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    public NotificationService(
        IEnumerable<INotificationChannel> channels,
        ILogger<NotificationService> logger,
        NotificationOptions options,
        ITemplateRenderer? templateRenderer = null,
        INotificationLogger? notificationLogger = null)
    {
        _channels = channels.ToDictionary(c => c.Channel);
        _logger = logger;
        _options = options;
        _templateRenderer = templateRenderer;
        _notificationLogger = notificationLogger;
    }

    /// <inheritdoc/>
    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var notificationId = Guid.NewGuid().ToString("N");
        var channelType = request.Channel ?? _options.DefaultChannel;

        if (!_channels.TryGetValue(channelType, out var channel))
        {
            _logger.LogWarning("No channel registered for {Channel}", channelType);
            return NotificationResult.Failure(notificationId, $"No channel registered for {channelType}");
        }

        try
        {
            var content = await ResolveContentAsync(request, cancellationToken).ConfigureAwait(false);

            var message = new NotificationMessage
            {
                Recipient = request.Recipient,
                Content = content,
                Priority = request.Priority,
                Metadata = request.Metadata,
                TemplateKey = request.TemplateKey,
                Parameters = request.Parameters
            };

            NotificationResult? result = null;
            for (var attempt = 0; attempt <= _options.MaxRetryAttempts; attempt++)
            {
                if (attempt > 0)
                {
                    _logger.LogWarning(
                        "Retrying notification {NotificationId}, attempt {Attempt}/{MaxAttempts}",
                        notificationId,
                        attempt,
                        _options.MaxRetryAttempts);
                    await Task.Delay(_options.RetryDelay, cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    result = await channel.SendAsync(message, cancellationToken).ConfigureAwait(false);
                    if (result.Status != NotificationStatus.Failed)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    result = NotificationResult.Failure(notificationId, ex.Message);
                }
            }

            result ??= NotificationResult.Failure(notificationId, "Notification failed after retry attempts.");

            if (_notificationLogger is not null)
            {
                await _notificationLogger.LogAsync(result, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send notification {NotificationId} via {Channel}", notificationId, channelType);
            var failedResult = NotificationResult.Failure(notificationId, ex.Message);

            if (_notificationLogger is not null)
            {
                await _notificationLogger.LogAsync(failedResult, cancellationToken).ConfigureAwait(false);
            }

            return failedResult;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NotificationResult>> SendAsync(IEnumerable<NotificationRequest> requests, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);

        var results = new List<NotificationResult>();
        foreach (var request in requests)
        {
            var result = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }

    private async Task<RenderedContent> ResolveContentAsync(NotificationRequest request, CancellationToken cancellationToken)
    {
        if (request.TemplateKey is not null)
        {
            if (_options.RenderingMode == RenderingMode.Delegated)
            {
                // Rendering is delegated to the channel - skip local rendering.
                // The channel reads TemplateKey and Parameters from NotificationMessage directly.
                return new RenderedContent(Subject: null, HtmlBody: null, PlainTextBody: null);
            }

            if (_templateRenderer is null)
            {
                throw new InvalidOperationException(
                    $"Template key '{request.TemplateKey}' was specified but no ITemplateRenderer is registered. " +
                    "Register a template renderer (e.g., AddScribanRenderer()) or configure " +
                    "RenderingMode.Delegated if rendering is handled by the channel.");
            }

            return await _templateRenderer.RenderAsync(request.TemplateKey, request.Parameters, cancellationToken).ConfigureAwait(false);
        }

        return new RenderedContent(request.Subject, request.Body, request.Body);
    }
}